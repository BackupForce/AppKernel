using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Campaigns;

public sealed class Campaign : Entity
{
    private readonly List<CampaignDraw> _draws = new();

    private Campaign(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        string name,
        DateTime grantOpenAtUtc,
        DateTime grantCloseAtUtc,
        CampaignStatus status,
        DateTime createdAtUtc) : base(id)
    {
        TenantId = tenantId;
        GameCode = gameCode;
        PlayTypeCode = playTypeCode;
        Name = name;
        GrantOpenAtUtc = grantOpenAtUtc;
        GrantCloseAtUtc = grantCloseAtUtc;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    private Campaign()
    {
    }

    public Guid TenantId { get; private set; }

    public GameCode GameCode { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public string Name { get; private set; }

    public DateTime GrantOpenAtUtc { get; private set; }

    public DateTime GrantCloseAtUtc { get; private set; }

    public CampaignStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<CampaignDraw> Draws => _draws.AsReadOnly();

    public static Result<Campaign> Create(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        string name,
        DateTime grantOpenAtUtc,
        DateTime grantCloseAtUtc,
        CampaignStatus status,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Campaign>(GamingErrors.CampaignNotFound);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Campaign>(GamingErrors.CampaignNameRequired);
        }

        if (grantOpenAtUtc >= grantCloseAtUtc)
        {
            return Result.Failure<Campaign>(GamingErrors.CampaignGrantWindowInvalid);
        }

        return new Campaign(
            Guid.NewGuid(),
            tenantId,
            gameCode,
            playTypeCode,
            name.Trim(),
            grantOpenAtUtc,
            grantCloseAtUtc,
            status,
            utcNow);
    }

    public Result AddDraw(Guid drawId, DateTime utcNow)
    {
        if (_draws.Any(item => item.DrawId == drawId))
        {
            return Result.Failure(GamingErrors.CampaignDrawDuplicated);
        }

        _draws.Add(CampaignDraw.Create(TenantId, Id, drawId, utcNow));
        return Result.Success();
    }
}
