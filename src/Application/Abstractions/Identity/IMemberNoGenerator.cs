namespace Application.Abstractions.Identity;

public interface IMemberNoGenerator
{
    Task<string> GenerateAsync(
        Guid tenantId,
        MemberNoGenerationMode mode,
        CancellationToken cancellationToken);
}

public enum MemberNoGenerationMode
{
    TenantPrefix,
    Timestamp
}
