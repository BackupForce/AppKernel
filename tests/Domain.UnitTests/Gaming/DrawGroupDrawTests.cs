using Domain.Gaming.DrawGroups;
using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Gaming;

public sealed class DrawGroupDrawTests
{
    [Fact]
    public void AddDraw_Should_Reject_Duplicated_Draw()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        DrawGroup drawGroup = DrawGroup.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "重複期數測試",
            now.AddHours(1),
            now.AddHours(2),
            DrawGroupStatus.Draft,
            now).Value;

        Guid drawId = Guid.NewGuid();
        Result firstAdd = drawGroup.AddDraw(drawId, now);
        Result secondAdd = drawGroup.AddDraw(drawId, now);

        firstAdd.IsSuccess.Should().BeTrue();
        secondAdd.IsFailure.Should().BeTrue();
        secondAdd.Error.Should().Be(GamingErrors.DrawGroupDrawDuplicated);
    }
}
