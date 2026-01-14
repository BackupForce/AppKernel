using Domain.Gaming;
using Domain.Gaming.Services;
using FluentAssertions;

namespace Domain.UnitTests.Gaming;

public sealed class PlayRuleRegistryTests
{
    [Fact]
    public void GetAllowedPlayTypes_Should_Return_PlayTypes_For_Game()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();

        IReadOnlyCollection<PlayTypeCode> playTypes = registry.GetAllowedPlayTypes(GameCodes.Lottery539);

        playTypes.Should().Contain(PlayTypeCodes.Basic);
    }
}
