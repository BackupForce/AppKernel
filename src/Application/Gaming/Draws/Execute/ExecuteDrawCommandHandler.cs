using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.Draws.Execute;

/// <summary>
/// 執行開獎流程：使用 Commit-Reveal 取得可驗證的中獎號碼。
/// </summary>
/// <remarks>
/// Application 層只依賴介面，RNG 與 seed 儲存由 Infrastructure 實作。
/// </remarks>
internal sealed class ExecuteDrawCommandHandler(
    IDrawRepository drawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    ILottery539RngService rngService,
    IServerSeedStore serverSeedStore) : ICommandHandler<ExecuteDrawCommand>
{
    public async Task<Result> Handle(ExecuteDrawCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        if (draw.Status == DrawStatus.Settled)
        {
            return Result.Failure(GamingErrors.DrawAlreadySettled);
        }

        DateTime now = dateTimeProvider.UtcNow;
        if (now < draw.DrawAt)
        {
            return Result.Failure(GamingErrors.DrawNotReadyToExecute);
        }

        if (string.IsNullOrWhiteSpace(draw.ServerSeedHash))
        {
            // 首次執行時建立 commit：先存 hash，避免事後更換 seed 影響公平性。
            string serverSeed = rngService.CreateServerSeed();
            string serverSeedHash = rngService.ComputeServerSeedHash(serverSeed);
            draw.OpenSales(serverSeedHash, now);
            TimeSpan ttl = draw.DrawAt > now ? draw.DrawAt - now + TimeSpan.FromDays(1) : TimeSpan.FromDays(1);
            await serverSeedStore.StoreAsync(draw.Id, serverSeed, ttl, cancellationToken);
        }

        string? serverSeedReveal = await serverSeedStore.GetAsync(draw.Id, cancellationToken);
        if (string.IsNullOrWhiteSpace(serverSeedReveal))
        {
            return Result.Failure(GamingErrors.ServerSeedMissing);
        }

        // 以 drawId 為 deterministic input，搭配 serverSeed 產生可重算的中獎號碼。
        Lottery539RngResult rngResult = rngService.GenerateWinningNumbers(draw.Id, serverSeedReveal);

        // 開獎揭露 serverSeed 並寫入 proof，讓外部可驗證 RNG 過程。
        draw.Execute(rngResult.Numbers, serverSeedReveal, rngResult.Algorithm, rngResult.DerivedInput, now);

        drawRepository.Update(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
