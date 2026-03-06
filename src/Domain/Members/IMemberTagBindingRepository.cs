namespace Domain.Members;

public interface IMemberTagBindingRepository
{
    Task<IReadOnlyCollection<Guid>> GetTagIdsByMemberIdAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task ReplaceMemberTagsAsync(
        Guid tenantId,
        Guid memberId,
        IReadOnlyCollection<Guid> tagIds,
        Guid? operatorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
