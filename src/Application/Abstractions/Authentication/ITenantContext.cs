namespace Application.Abstractions.Authentication;

public interface ITenantContext
{
    Guid TenantId { get; }
}
