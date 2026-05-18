namespace Application.Abstractions.Authentication;

public interface ICurrentSession
{
    bool IsAuthenticated { get; }

    Guid? TenantId { get; }

    Guid? AuthSessionId { get; }
}
