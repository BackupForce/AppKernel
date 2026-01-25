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
/// 下注流程：驗證期數、建立票券並提交號碼。
/// </summary>
internal sealed class PlaceTicketCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    ITicketTemplateRepository ticketTemplateRepository,
    IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository,
    IMemberRepository memberRepository,
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

        if (request.Lines.Count > 1 || request.Lines.Count > template.MaxLinesPerTicket)
        {
            return Result.Failure<Guid>(GamingErrors.TicketLinesExceedLimit);
        }

        Ticket ticket = Ticket.Create(
            tenantContext.TenantId,
            draw.GameCode,
            member.Id,
            null,
            template.Id,
            draw.Id,
            null,
            null,
            now,
            IssuedByType.System,
            userContext.UserId,
            "direct_place",
            null,
            now);

        Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(request.Lines.First());
        if (numbersResult.IsFailure)
        {
            return Result.Failure<Guid>(numbersResult.Error);
        }

        Result validateResult = rule.ValidateBet(numbersResult.Value);
        if (validateResult.IsFailure)
        {
            return Result.Failure<Guid>(validateResult.Error);
        }

        Result submitResult = ticket.SubmitNumbers(playTypeCode, numbersResult.Value, now, userContext.UserId, null, null);
        if (submitResult.IsFailure)
        {
            return Result.Failure<Guid>(submitResult.Error);
        }

        TicketLine line = ticket.Lines.Single();
        TicketDraw ticketDraw = TicketDraw.Create(tenantContext.TenantId, ticket.Id, draw.Id, now);
        if (draw.IsWithinSalesWindow(now))
        {
            ticketDraw.MarkActive(now);
        }
        else
        {
            ticketDraw.MarkInvalid(now);
        }

        drawRepository.Update(draw);
        ticketRepository.Insert(ticket);
        ticketDrawRepository.Insert(ticketDraw);

        ticketRepository.InsertLine(line);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }
}
