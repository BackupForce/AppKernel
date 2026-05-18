using Domain.Gaming.RedeemRecords;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RedeemRecordRepository(ApplicationDbContext context) : IRedeemRecordRepository
{
    public async Task<RedeemRecord?> GetByAwardIdAsync(Guid prizeAwardId, CancellationToken cancellationToken = default)
    {
        return await context.RedeemRecords
            .FirstOrDefaultAsync(record => record.PrizeAwardId == prizeAwardId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid prizeAwardId, CancellationToken cancellationToken = default)
    {
        return await context.RedeemRecords.AnyAsync(record => record.PrizeAwardId == prizeAwardId, cancellationToken);
    }

    public void Insert(RedeemRecord record)
    {
        context.RedeemRecords.Add(record);
    }
}
