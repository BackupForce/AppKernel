namespace Domain.Members;

public interface IMemberTagCatalogRepository
{
    Task<MemberTag?> GetByIdAsync(
        Guid tenantId,
        Guid tagId,
        CancellationToken cancellationToken = default);

    Task<MemberTag?> GetByCodeAsync(
        Guid tenantId,
        string tagCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MemberTag>> GetByIdsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken = default);

    void Insert(MemberTag tag);

    void Update(MemberTag tag);
}
