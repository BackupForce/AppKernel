using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Gaming;

public sealed class DrawPlayTypeTests
{
    [Fact]
    public void EnablePlayTypes_Should_Reject_PlayType_Not_In_Game()
    {
        Draw draw = CreateDraw();
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();

        Result result = draw.EnablePlayTypes(new[] { new PlayTypeCode("VIP") }, registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.PlayTypeNotAllowed);
    }

    [Fact]
    public void ConfigurePrizeOption_Should_Reject_Unenabled_PlayType()
    {
        Draw draw = CreateDraw();
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        PrizeOption option = PrizeOption.Create("T4獎", 100m, null, "測試").Value;

        Result result = draw.ConfigurePrizeOption(PlayTypeCodes.Basic, new PrizeTier("T4"), option, registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.TicketPlayTypeNotEnabled);
    }

    [Fact]
    public void ConfigurePrizeOption_Should_Reject_Tier_Not_In_Rule()
    {
        Draw draw = CreateDraw();
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, registry);
        PrizeOption option = PrizeOption.Create("未知獎", 50m, null, null).Value;

        Result result = draw.ConfigurePrizeOption(PlayTypeCodes.Basic, new PrizeTier("T99"), option, registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.PrizeTierNotAllowed);
    }

    [Fact]
    public void EnsurePrizePoolCompleteForSettlement_Should_Fail_When_Tier_Missing()
    {
        Draw draw = CreateDraw();
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, registry);
        PrizeOption option = PrizeOption.Create("頭獎", 5000m, null, null).Value;
        draw.ConfigurePrizeOption(PlayTypeCodes.Basic, new PrizeTier("T1"), option, registry);

        Result result = draw.EnsurePrizePoolCompleteForSettlement(registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.PrizePoolIncomplete);
    }

    private static Draw CreateDraw()
    {
        DateTime now = DateTime.UtcNow;
        Result<Draw> drawResult = Draw.Create(
            Guid.NewGuid(),
            GameCodes.Lottery539,
            now.AddHours(1),
            now.AddHours(2),
            now.AddHours(3),
            DrawStatus.Scheduled,
            null,
            now);
        return drawResult.Value;
    }
}
