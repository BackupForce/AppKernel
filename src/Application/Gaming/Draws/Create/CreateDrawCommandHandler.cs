using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Draws.Create;

/// <summary>
/// 建立期數並在需要時預先寫入 commit hash。
/// </summary>
/// <remarks>
/// 僅協調時間與持久化，RNG 由 Infrastructure 實作。
/// </remarks>
internal sealed class CreateDrawCommandHandler(
    IDrawRepository drawRepository,
    IDrawCodeGenerator drawCodeGenerator,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    ILottery539RngService rngService,
    IServerSeedStore serverSeedStore,
    IEntitlementChecker entitlementChecker) : ICommandHandler<CreateDrawCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDrawCommand request, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;
        DrawStatus initialStatus = DrawStatus.Scheduled;

        // 根據目前時間推導初始狀態，避免新建立即處於不一致狀態。
        if (now >= request.SalesStartAt && now < request.SalesCloseAt)
        {
            initialStatus = DrawStatus.SalesOpen;
        }
        else if (now >= request.SalesCloseAt && now < request.DrawAt)
        {
            initialStatus = DrawStatus.SalesClosed;
        }
        else if (now >= request.DrawAt)
        {
            initialStatus = DrawStatus.Settled;
        }

        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<Guid>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<Guid>(entitlementResult.Error);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        string drawCode = await drawCodeGenerator.IssueDrawCodeAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            request.DrawAt,
            now,
            cancellationToken);
        string? normalizedDrawCode = string.IsNullOrWhiteSpace(drawCode)
            ? null
            : drawCode.Trim();
        if (normalizedDrawCode is null)
        {
            return Result.Failure<Guid>(GamingErrors.DrawCodeRequired);
        }

        Result<Draw> drawResult = Draw.Create(
            tenantContext.TenantId,
            gameCodeResult.Value,
            normalizedDrawCode,
            request.SalesStartAt,
            request.SalesCloseAt,
            request.DrawAt,
            initialStatus,
            request.RedeemValidDays,
            now,
            registry);

        if (drawResult.IsFailure)
        {
            return Result.Failure<Guid>(drawResult.Error);
        }

        Draw draw = drawResult.Value;

        if (request.EnabledPlayTypes.Count > 0)
        {
            List<PlayTypeCode> playTypes = new List<PlayTypeCode>();
            foreach (string playType in request.EnabledPlayTypes)
            {
                Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(playType);
                if (playTypeResult.IsFailure)
                {
                    return Result.Failure<Guid>(playTypeResult.Error);
                }

                Result playEntitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
                    tenantContext.TenantId,
                    draw.GameCode,
                    playTypeResult.Value,
                    cancellationToken);
                if (playEntitlementResult.IsFailure)
                {
                    return Result.Failure<Guid>(playEntitlementResult.Error);
                }

                playTypes.Add(playTypeResult.Value);
            }

            Result enableResult = draw.EnablePlayTypes(playTypes, registry);
            if (enableResult.IsFailure)
            {
                return Result.Failure<Guid>(enableResult.Error);
            }
        }

        if (initialStatus == DrawStatus.SalesOpen)
        {
            // 若建立時已在銷售期，先寫入 commit hash 以支持後續驗證。
            string serverSeed = rngService.CreateServerSeed();
            string serverSeedHash = rngService.ComputeServerSeedHash(serverSeed);
            draw.OpenSales(serverSeedHash, now);
            TimeSpan ttl = request.DrawAt > now ? request.DrawAt - now + TimeSpan.FromDays(1) : TimeSpan.FromDays(1);
            await serverSeedStore.StoreAsync(draw.Id, serverSeed, ttl, cancellationToken);
        }

        drawRepository.Insert(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return draw.Id;
    }
}
