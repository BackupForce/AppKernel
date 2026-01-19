using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Gaming.TicketTemplates;
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
    ITicketTemplateRepository ticketTemplateRepository,
    IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository,
    IMemberRepository memberRepository,
    IWalletLedgerService walletLedgerService,
    IServerSeedStore serverSeedStore,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    ILottery539RngService rngService,
    IEntitlementChecker entitlementChecker) : ICommandHandler<PlaceTicketCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceTicketCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<Guid>(GamingErrors.DrawNotFound);
        }

        Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(request.PlayTypeCode);
        if (playTypeResult.IsFailure)
        {
            return Result.Failure<Guid>(playTypeResult.Error);
        }

        PlayTypeCode playTypeCode = playTypeResult.Value;
        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            playTypeCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<Guid>(entitlementResult.Error);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        if (!registry.GetAllowedPlayTypes(draw.GameCode).Contains(playTypeCode))
        {
            return Result.Failure<Guid>(GamingErrors.PlayTypeNotAllowed);
        }

        if (!draw.EnabledPlayTypes.Contains(playTypeCode))
        {
            return Result.Failure<Guid>(GamingErrors.TicketPlayTypeNotEnabled);
        }

        IPlayRule rule = registry.GetRule(draw.GameCode, playTypeCode);

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

        if (draw.IsManuallyClosed)
        {
            return Result.Failure<Guid>(GamingErrors.DrawManuallyClosed);
        }

        if (!draw.IsWithinSalesWindow(now) || draw.Status != DrawStatus.SalesOpen)
        {
            return Result.Failure<Guid>(GamingErrors.DrawNotOpen);
        }

        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<Guid>(GamingErrors.MemberNotFound);
        }

        TicketTemplate? template = await ticketTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure<Guid>(GamingErrors.TicketTemplateNotFound);
        }

        if (!template.IsActive)
        {
            return Result.Failure<Guid>(GamingErrors.TicketTemplateInactive);
        }

        if (!template.IsAvailable(now))
        {
            return Result.Failure<Guid>(GamingErrors.TicketTemplateNotAvailable);
        }

        IReadOnlyCollection<DrawAllowedTicketTemplate> allowedTemplates =
            await drawAllowedTicketTemplateRepository.GetByDrawIdAsync(
                tenantContext.TenantId,
                draw.Id,
                cancellationToken);

        if (allowedTemplates.Count == 0)
        {
            // 中文註解：未設定允許票種時採預設拒絕，避免未經核准的票種被使用。
            return Result.Failure<Guid>(GamingErrors.TicketTemplateNotAllowed);
        }

        bool isAllowed = allowedTemplates.Any(item => item.TicketTemplateId == template.Id);
        if (!isAllowed)
        {
            return Result.Failure<Guid>(GamingErrors.TicketTemplateNotAllowed);
        }

        if (request.Lines.Count == 0)
        {
            return Result.Failure<Guid>(GamingErrors.TicketLineInvalid);
        }

        if (request.Lines.Count > template.MaxLinesPerTicket)
        {
            return Result.Failure<Guid>(GamingErrors.TicketLinesExceedLimit);
        }

        // 中文註解：每注價格以模板單價計算，總成本 = 單價 * 注數。
        decimal totalCostDecimal = template.Price * request.Lines.Count;
        if (decimal.Truncate(totalCostDecimal) != totalCostDecimal)
        {
            // 中文註解：帳本點數以整數為單位，若出現小數價格直接拒絕。
            return Result.Failure<Guid>(GamingErrors.TicketTemplatePriceInvalid);
        }

        long totalCost = (long)totalCostDecimal;
        // 中文註解：PriceSnapshot 記錄下單當下的模板單價，避免日後調價影響稽核。
        Ticket ticket = Ticket.Create(
            tenantContext.TenantId,
            draw.Id,
            draw.GameCode,
            playTypeCode,
            member.Id,
            template.Id,
            template.Price,
            totalCost,
            now);

        int lineIndex = 0;
        foreach (IReadOnlyCollection<int> lineNumbers in request.Lines)
        {
            Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(lineNumbers);
            if (numbersResult.IsFailure)
            {
                return Result.Failure<Guid>(numbersResult.Error);
            }

            Result validateResult = rule.ValidateBet(numbersResult.Value);
            if (validateResult.IsFailure)
            {
                return Result.Failure<Guid>(validateResult.Error);
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
