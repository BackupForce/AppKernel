using Domain.Gaming;
using Domain.Gaming.Services;
using FluentAssertions;

namespace Domain.UnitTests.Gaming;

public sealed class Lottery539BasicPlayRuleTests
{
    [Fact]
    public void Evaluate_Should_Return_Tier4_When_Two_Matched()
    {
        Lottery539BasicPlayRule rule = new Lottery539BasicPlayRule();
        LotteryNumbers bet = LotteryNumbers.Create(new[] { 1, 2, 6, 7, 8 }).Value;
        LotteryNumbers result = LotteryNumbers.Create(new[] { 1, 2, 3, 4, 5 }).Value;

        PrizeTier? tier = rule.Evaluate(bet, result);

        tier.Should().NotBeNull();
        tier.Should().Be(new PrizeTier("T4"));
    }
}
