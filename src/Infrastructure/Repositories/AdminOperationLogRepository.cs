using Domain.Admin.OperationLogs;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class AdminOperationLogRepository(ApplicationDbContext context) : IAdminOperationLogRepository
{
    public Task AddAsync(AdminOperationLog log, CancellationToken cancellationToken = default)
    {
        return context.AdminOperationLogs.AddAsync(log, cancellationToken).AsTask();
    }

    public Task<bool> ExistsByDedupKeyAsync(string dedupKey, CancellationToken cancellationToken = default)
    {
        return context.AdminOperationLogs.AnyAsync(log => log.DedupKey == dedupKey, cancellationToken);
    }
}
