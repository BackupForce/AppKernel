using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using FluentAssertions;
using Microsoft.Win32;
using SharedKernel;

namespace Domain.UnitTests.Gaming;

public sealed class DrawPlayTypeTests
{
    [Fact]
    public void EnablePlayTypes_Should_Reject_PlayType_Not_In_Game()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);

        Result result = draw.EnablePlayTypes(new[] { new PlayTypeCode("VIP") }, registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.PlayTypeNotAllowed);
    }

    [Fact]
    public void ConfigurePrizeOption_Should_Reject_Unenabled_PlayType()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);
        PrizeOption option = PrizeOption.Create("T4獎", 100m, null, "測試").Value;

        Result result = draw.ConfigurePrizeOption(PlayTypeCodes.Basic, new PrizeTier("T4"), option, registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.TicketPlayTypeNotEnabled);
    }

    [Fact]
    public void ConfigurePrizeOption_Should_Reject_Tier_Not_In_Rule()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, registry);
        PrizeOption option = PrizeOption.Create("未知獎", 50m, null, null).Value;

        Result result = draw.ConfigurePrizeOption(PlayTypeCodes.Basic, new PrizeTier("T99"), option, registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.PrizeTierNotAllowed);
    }

    [Fact]
    public void EnsurePrizePoolCompleteForSettlement_Should_Fail_When_Tier_Missing()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, registry);
        PrizeOption option = PrizeOption.Create("頭獎", 5000m, null, null).Value;
        draw.ConfigurePrizeOption(PlayTypeCodes.Basic, new PrizeTier("T1"), option, registry);

        Result result = draw.EnsurePrizePoolCompleteForSettlement(registry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.PrizePoolIncomplete);
    }

    private static Draw CreateDraw(PlayRuleRegistry registry)
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
            now,
            registry);
        return drawResult.Value;
    }
}
