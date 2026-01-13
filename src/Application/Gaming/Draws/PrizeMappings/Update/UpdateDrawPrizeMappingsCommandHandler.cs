using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.Draws.PrizeMappings.Update;

/// <summary>
/// 更新期數獎項對應設定。
/// </summary>
/// <remarks>
/// 覆寫式更新確保後台操作可重送且不會累積重複資料。
/// </remarks>
internal sealed class UpdateDrawPrizeMappingsCommandHandler(
    IDrawRepository drawRepository,
    IDrawPrizeMappingRepository drawPrizeMappingRepository,
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdateDrawPrizeMappingsCommand>
{
    public async Task<Result> Handle(UpdateDrawPrizeMappingsCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        IReadOnlyCollection<DrawPrizeMapping> existing = await drawPrizeMappingRepository.GetByDrawIdAsync(
            tenantContext.TenantId,
            draw.Id,
            cancellationToken);

        drawPrizeMappingRepository.RemoveRange(existing);

        DateTime now = dateTimeProvider.UtcNow;
        HashSet<(int MatchCount, Guid PrizeId)> inserted = new HashSet<(int MatchCount, Guid PrizeId)>();

        foreach (DrawPrizeMappingInput mappingInput in request.Mappings)
        {
            foreach (Guid prizeId in mappingInput.PrizeIds)
            {
                if (inserted.Contains((mappingInput.MatchCount, prizeId)))
                {
                    continue;
                }

                Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, prizeId, cancellationToken);
                if (prize is null)
                {
                    return Result.Failure(GamingErrors.PrizeNotFound);
                }

                Result<DrawPrizeMapping> mappingResult = DrawPrizeMapping.Create(
                    tenantContext.TenantId,
                    draw.Id,
                    mappingInput.MatchCount,
                    prizeId,
                    now);

                if (mappingResult.IsFailure)
                {
                    return Result.Failure(mappingResult.Error);
                }

                drawPrizeMappingRepository.Insert(mappingResult.Value);
                inserted.Add((mappingInput.MatchCount, prizeId));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
