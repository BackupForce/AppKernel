using SharedKernel;

namespace Domain.Members;

public sealed class MemberActivityLog : Entity
{
    private MemberActivityLog(
        Guid id,
        Guid memberId,
        string action,
        string? ip,
        string? userAgent,
        Guid? operatorUserId,
        string? payload,
        DateTime createdAt) : base(id)
    {
        MemberId = memberId;
        Action = action;
        Ip = ip;
        UserAgent = userAgent;
        OperatorUserId = operatorUserId;
        Payload = payload;
        CreatedAt = createdAt;
    }

    private MemberActivityLog()
    {
    }

    public Guid MemberId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string? Ip { get; private set; }

    public string? UserAgent { get; private set; }

    public Guid? OperatorUserId { get; private set; }

    public string? Payload { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static MemberActivityLog Create(
        Guid memberId,
        string action,
        string? ip,
        string? userAgent,
        Guid? operatorUserId,
        string? payload,
        DateTime createdAt)
    {
        return new MemberActivityLog(
            Guid.NewGuid(),
            memberId,
            action,
            ip,
            userAgent,
            operatorUserId,
            payload,
            createdAt);
    }
}
