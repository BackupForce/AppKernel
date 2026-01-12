using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.Draws.Execute;

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

        Lottery539RngResult rngResult = rngService.GenerateWinningNumbers(draw.Id, serverSeedReveal);

        // 中文註解：開獎揭露 serverSeed 並寫入 proof，讓外部可驗證 RNG 過程。
        draw.Execute(rngResult.Numbers, serverSeedReveal, rngResult.Algorithm, rngResult.DerivedInput, now);

        drawRepository.Update(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
