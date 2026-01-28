using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Gaming;

public sealed class DrawTests
{
    [Fact]
    public void Create_Should_Require_DrawCode()
    {
        DateTime now = DateTime.UtcNow;

        Result<Draw> result = Draw.Create(
            Guid.NewGuid(),
            GameCodes.Lottery539,
            string.Empty,
            now.AddHours(1),
            now.AddHours(2),
            now.AddHours(3),
            null,
            now,
            PlayRuleRegistry.CreateDefault());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.DrawCodeRequired);
    }
}
