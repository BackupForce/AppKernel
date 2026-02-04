using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.DrawGroups;

public sealed class DrawGroup : Entity
{
    private readonly List<DrawGroupDraw> _draws = new();

    private DrawGroup(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        string name,
        DateTime? grantOpenAtUtc,
        DateTime? grantCloseAtUtc,
        DrawGroupStatus status,
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

    private DrawGroup()
    {
    }

    public Guid TenantId { get; private set; }

    public GameCode GameCode { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public string Name { get; private set; }

    public DateTime? GrantOpenAtUtc { get; private set; }

    public DateTime? GrantCloseAtUtc { get; private set; }

    public DrawGroupStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<DrawGroupDraw> Draws => _draws;

    public static Result<DrawGroup> Create(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        string name,
        DrawGroupStatus status,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<DrawGroup>(GamingErrors.DrawGroupNotFound);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<DrawGroup>(GamingErrors.DrawGroupNameRequired);
        }

        return new DrawGroup(
            Guid.NewGuid(),
            tenantId,
            gameCode,
            playTypeCode,
            name.Trim(),
            null,
            null,
            status,
            utcNow);
    }

    public Result AddDraw(Guid drawId, DateTime utcNow, IReadOnlyCollection<DrawGrantWindow> grantWindows)
    {
        if (_draws.Any(item => item.DrawId == drawId))
        {
            return Result.Failure(GamingErrors.DrawGroupDrawDuplicated);
        }

        _draws.Add(DrawGroupDraw.Create(TenantId, Id, drawId, utcNow));
        RecalculateGrantWindow(grantWindows);
        return Result.Success();
    }

    public Result Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(GamingErrors.DrawGroupNameRequired);
        }

        Name = name.Trim();
        return Result.Success();
    }

    public Result Activate(DateTime utcNow)
    {
        if (Status != DrawGroupStatus.Draft)
        {
            return Result.Failure(GamingErrors.DrawGroupNotDraft);
        }

        if (_draws.Count == 0)
        {
            return Result.Failure(GamingErrors.DrawGroupDrawRequired);
        }

        Status = DrawGroupStatus.Active;
        return Result.Success();
    }

    public Result End(DateTime utcNow)
    {
        if (Status != DrawGroupStatus.Active)
        {
            return Result.Failure(GamingErrors.DrawGroupNotActive);
        }

        Status = DrawGroupStatus.Ended;
        return Result.Success();
    }

    public Result RemoveDraw(Guid drawId, IReadOnlyCollection<DrawGrantWindow> grantWindows)
    {
        if (Status != DrawGroupStatus.Draft)
        {
            return Result.Failure(GamingErrors.DrawGroupNotDraft);
        }

        DrawGroupDraw? existing = _draws.Find(item => item.DrawId == drawId);
        if (existing is null)
        {
            return Result.Failure(GamingErrors.DrawGroupDrawNotFound);
        }

        _draws.Remove(existing);
        RecalculateGrantWindow(grantWindows);
        return Result.Success();
    }

    private void RecalculateGrantWindow(IReadOnlyCollection<DrawGrantWindow> grantWindows)
    {
        if (grantWindows is null || grantWindows.Count == 0)
        {
            GrantOpenAtUtc = null;
            GrantCloseAtUtc = null;
            return;
        }

        GrantOpenAtUtc = grantWindows.Min(item => item.OpenAtUtc);
        GrantCloseAtUtc = grantWindows.Max(item => item.CloseAtUtc);
    }
}
