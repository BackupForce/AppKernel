using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using Domain.Gaming.Services;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Place;

/// <summary>
/// 下注流程：驗證期數、建立票券、扣點並持久化。
/// </summary>
/// <remarks>
/// Application 層透過 IWalletLedgerService 等介面與外部系統互動，確保依賴方向正確。
/// </remarks>
internal sealed class PlaceTicketCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    IMemberRepository memberRepository,
    IWalletLedgerService walletLedgerService,
    IServerSeedStore serverSeedStore,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    ILottery539RngService rngService) : ICommandHandler<PlaceTicketCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceTicketCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<Guid>(GamingErrors.DrawNotFound);
        }

        DateTime now = dateTimeProvider.UtcNow;

        if (now >= draw.SalesCloseAt)
        {
            draw.CloseSales(now);
        }

        if (draw.Status == DrawStatus.Scheduled && now >= draw.SalesOpenAt && now < draw.SalesCloseAt)
        {
            // 首次進入販售狀態時建立 commit hash，為後續開獎公平性做準備。
            string serverSeed = rngService.CreateServerSeed();
            string serverSeedHash = rngService.ComputeServerSeedHash(serverSeed);
            draw.OpenSales(serverSeedHash, now);
            TimeSpan ttl = draw.DrawAt > now ? draw.DrawAt - now + TimeSpan.FromDays(1) : TimeSpan.FromDays(1);
            await serverSeedStore.StoreAsync(draw.Id, serverSeed, ttl, cancellationToken);
        }

        if (draw.Status != DrawStatus.SalesOpen || now >= draw.SalesCloseAt)
        {
            return Result.Failure<Guid>(GamingErrors.DrawNotOpen);
        }

        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<Guid>(GamingErrors.MemberNotFound);
        }

        if (request.Lines.Count == 0)
        {
            return Result.Failure<Guid>(GamingErrors.TicketLineInvalid);
        }

        // 每注固定成本，總成本用於帳本扣點與後續報表。
        long totalCost = request.Lines.Count * Lottery539GameConfig.LineCost;
        Ticket ticket = Ticket.Create(tenantContext.TenantId, draw.Id, member.Id, totalCost, now);

        int lineIndex = 0;
        foreach (IReadOnlyCollection<int> lineNumbers in request.Lines)
        {
            Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(lineNumbers);
            if (numbersResult.IsFailure)
            {
                return Result.Failure<Guid>(numbersResult.Error);
            }

            Result<TicketLine> lineResult = TicketLine.Create(ticket.Id, lineIndex, numbersResult.Value);
            if (lineResult.IsFailure)
            {
                return Result.Failure<Guid>(lineResult.Error);
            }

            TicketLine line = lineResult.Value;
            ticket.AddLine(line);
            lineIndex++;
        }

        // 下注與扣點需在同一交易中完成，避免票券落地但扣點失敗。
        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        // reference 使用 ticket.Id，避免外部帳本重複扣點。
        Result<long> debitResult = await walletLedgerService.DebitAsync(
            tenantContext.TenantId,
            member.Id,
            totalCost,
            "gaming_ticket",
            ticket.Id.ToString(),
            "539 下注扣點",
            cancellationToken);

        if (debitResult.IsFailure)
        {
            return Result.Failure<Guid>(debitResult.Error);
        }

        drawRepository.Update(draw);
        ticketRepository.Insert(ticket);

        foreach (TicketLine line in ticket.Lines)
        {
            ticketRepository.InsertLine(line);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        return ticket.Id;
    }
}
