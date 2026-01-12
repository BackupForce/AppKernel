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

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

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
