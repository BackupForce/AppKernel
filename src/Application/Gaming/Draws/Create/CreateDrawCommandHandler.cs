using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming;
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
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    ILottery539RngService rngService,
    IServerSeedStore serverSeedStore) : ICommandHandler<CreateDrawCommand, Guid>
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

        Result<Draw> drawResult = Draw.Create(
            tenantContext.TenantId,
            request.SalesStartAt,
            request.SalesCloseAt,
            request.DrawAt,
            initialStatus,
            request.RedeemValidDays,
            now);

        if (drawResult.IsFailure)
        {
            return Result.Failure<Guid>(drawResult.Error);
        }

        Draw draw = drawResult.Value;

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
