using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.Draws.Create;

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

        if (now >= request.SalesOpenAt && now < request.SalesCloseAt)
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
            request.SalesOpenAt,
            request.SalesCloseAt,
            request.DrawAt,
            initialStatus,
            now);

        if (drawResult.IsFailure)
        {
            return Result.Failure<Guid>(drawResult.Error);
        }

        Draw draw = drawResult.Value;

        if (initialStatus == DrawStatus.SalesOpen)
        {
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
