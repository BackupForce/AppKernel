using System.Reflection;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Gaming;

public sealed class DrawSettlementTests
{
    [Fact]
    public void MarkSettled_Should_Set_EffectiveStatus_When_Drawn()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);
        DateTime drawnAt = DateTime.UtcNow;

        LotteryNumbers winningNumbers = LotteryNumbers.Create(new[] { 1, 2, 3, 4, 5 }).Value;
        draw.Execute(winningNumbers, "seed", "algo", "input", drawnAt);

        DateTime settledAt = drawnAt.AddMinutes(5);
        draw.MarkSettled(settledAt);

        draw.GetEffectiveStatus(settledAt).Should().Be(DrawStatus.Settled);
        draw.SettledAtUtc.Should().Be(settledAt);
    }

    [Fact]
    public void MarkSettled_Should_Be_Idempotent()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);
        DateTime drawnAt = DateTime.UtcNow;

        LotteryNumbers winningNumbers = LotteryNumbers.Create(new[] { 1, 2, 3, 4, 5 }).Value;
        draw.Execute(winningNumbers, "seed", "algo", "input", drawnAt);

        DateTime firstSettledAt = drawnAt.AddMinutes(1);
        DateTime secondSettledAt = drawnAt.AddMinutes(2);

        draw.MarkSettled(firstSettledAt);
        draw.MarkSettled(secondSettledAt);

        draw.SettledAtUtc.Should().Be(firstSettledAt);
    }

    [Fact]
    public void MarkSettled_Should_Throw_When_Not_Drawn()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);

        Action action = () => draw.MarkSettled(DateTime.UtcNow);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkSettled_Should_Throw_When_Cancelled()
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Draw draw = CreateDraw(registry);

        PropertyInfo? statusProperty = typeof(Draw).GetProperty(
            "Status",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        statusProperty.Should().NotBeNull();
        statusProperty!.SetValue(draw, DrawStatus.Cancelled);

        Action action = () => draw.MarkSettled(DateTime.UtcNow);

        action.Should().Throw<InvalidOperationException>();
    }

    private static Draw CreateDraw(PlayRuleRegistry registry)
    {
        DateTime now = DateTime.UtcNow;
        Result<Draw> drawResult = Draw.Create(
            Guid.NewGuid(),
            GameCodes.Lottery539,
            "539-2401001",
            now.AddHours(1),
            now.AddHours(2),
            now.AddHours(3),
            null,
            now,
            registry);
        return drawResult.Value;
    }
}
