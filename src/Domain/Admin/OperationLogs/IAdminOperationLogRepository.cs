namespace Domain.Admin.OperationLogs;

public interface IAdminOperationLogRepository
{
    Task AddAsync(AdminOperationLog log, CancellationToken cancellationToken = default);

    Task<bool> ExistsByDedupKeyAsync(string dedupKey, CancellationToken cancellationToken = default);
}
