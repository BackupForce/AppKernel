using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Gaming.Draws.GetScheduledOrSalesOpen;
using Application.IntegrationTests.Infrastructure;
using Domain.Gaming.Draws;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application.IntegrationTests.Gaming;

public sealed class GetScheduledOrSalesOpenDrawsTests : BaseIntegrationTest
{
    public GetScheduledOrSalesOpenDrawsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Scheduled_Or_SalesOpen_Draws_With_EffectiveStatus()
    {
        DateTime now = new(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc);
        Guid tenantId = Guid.NewGuid();
        string gameCode = "539";

        Guid scheduledDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            scheduledDrawId,
            gameCode,
            "539-260322001",
            salesOpenAt: now.AddHours(1),
            salesCloseAt: now.AddHours(3),
            drawAt: now.AddHours(4),
            status: DrawStatus.Scheduled);

        Guid salesOpenDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            salesOpenDrawId,
            gameCode,
            "539-260322002",
            salesOpenAt: now.AddHours(-1),
            salesCloseAt: now.AddHours(2),
            drawAt: now.AddHours(3),
            status: DrawStatus.SalesOpen);

        Guid manuallyClosedDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            manuallyClosedDrawId,
            gameCode,
            "539-closed-manual",
            salesOpenAt: now.AddHours(-2),
            salesCloseAt: now.AddHours(1),
            drawAt: now.AddHours(3),
            status: DrawStatus.SalesOpen,
            isManuallyClosed: true,
            manualCloseAt: now.AddMinutes(-30));

        Guid drawnAtDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            drawnAtDrawId,
            gameCode,
            "539-drawn-at",
            salesOpenAt: now.AddHours(-2),
            salesCloseAt: now.AddHours(1),
            drawAt: now.AddHours(2),
            status: DrawStatus.SalesOpen,
            drawnAt: now.AddMinutes(-5));

        Guid winningNumbersDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            winningNumbersDrawId,
            gameCode,
            "539-winning-numbers",
            salesOpenAt: now.AddHours(-2),
            salesCloseAt: now.AddHours(1),
            drawAt: now.AddHours(2),
            status: DrawStatus.SalesOpen,
            winningNumbersRaw: "01,02,03,04,05");

        Guid settledDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            settledDrawId,
            gameCode,
            "539-settled",
            salesOpenAt: now.AddHours(-3),
            salesCloseAt: now.AddHours(-1),
            drawAt: now.AddMinutes(-30),
            status: DrawStatus.SalesClosed,
            settledAtUtc: now.AddMinutes(-10));

        Guid cancelledDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            cancelledDrawId,
            gameCode,
            "539-cancelled",
            salesOpenAt: now.AddHours(2),
            salesCloseAt: now.AddHours(4),
            drawAt: now.AddHours(5),
            status: DrawStatus.Cancelled);

        Guid salesClosedDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            tenantId,
            salesClosedDrawId,
            gameCode,
            "539-sales-closed",
            salesOpenAt: now.AddHours(-3),
            salesCloseAt: now,
            drawAt: now.AddHours(1),
            status: DrawStatus.SalesClosed);

        Guid otherTenantDrawId = Guid.NewGuid();
        await InsertDrawAsync(
            Guid.NewGuid(),
            otherTenantDrawId,
            gameCode,
            "539-other-tenant",
            salesOpenAt: now.AddHours(-1),
            salesCloseAt: now.AddHours(1),
            drawAt: now.AddHours(2),
            status: DrawStatus.SalesOpen);

        await using AsyncServiceScope scope = ServiceProvider.CreateAsyncScope();
        var handler = new GetScheduledOrSalesOpenDrawsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>(),
            new TestTenantContext(tenantId),
            new FixedDateTimeProvider(now));

        Result<IReadOnlyCollection<ScheduledOrSalesOpenDrawDto>> result = await handler.Handle(
            new GetScheduledOrSalesOpenDrawsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        IReadOnlyCollection<Guid> returnedIds = result.Value.Select(x => x.Id).ToArray();
        returnedIds.Should().ContainInOrder(scheduledDrawId, salesOpenDrawId);
        returnedIds.Should().NotContain(new[]
        {
            manuallyClosedDrawId,
            drawnAtDrawId,
            winningNumbersDrawId,
            settledDrawId,
            cancelledDrawId,
            salesClosedDrawId,
            otherTenantDrawId
        });

        result.Value.Single(x => x.Id == scheduledDrawId).EffectiveStatus.Should().Be("Scheduled");
        result.Value.Single(x => x.Id == salesOpenDrawId).EffectiveStatus.Should().Be("SalesOpen");
    }

    private async Task InsertDrawAsync(
        Guid tenantId,
        Guid drawId,
        string gameCode,
        string drawCode,
        DateTime salesOpenAt,
        DateTime salesCloseAt,
        DateTime drawAt,
        DrawStatus status,
        bool isManuallyClosed = false,
        DateTime? manualCloseAt = null,
        DateTime? drawnAt = null,
        string? winningNumbersRaw = null,
        DateTime? settledAtUtc = null)
    {
        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.draws (
                id,
                tenant_id,
                game_code,
                sales_open_at,
                draw_code,
                sales_close_at,
                draw_at,
                status,
                winning_numbers_raw,
                server_seed_hash,
                server_seed,
                algorithm,
                derived_input,
                is_manually_closed,
                manual_close_at,
                manual_close_reason,
                settled_at,
                settled_at_utc,
                redeem_valid_days,
                created_at,
                updated_at,
                source_template_id,
                source_template_version)
            VALUES (
                {drawId},
                {tenantId},
                {gameCode},
                {salesOpenAt},
                {drawCode},
                {salesCloseAt},
                {drawAt},
                {(int)status},
                {winningNumbersRaw},
                NULL,
                NULL,
                NULL,
                NULL,
                {isManuallyClosed},
                {manualCloseAt},
                NULL,
                {drawnAt},
                {settledAtUtc},
                NULL,
                {salesOpenAt},
                {salesOpenAt},
                NULL,
                NULL);
            """);
    }

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow => utcNow;
    }

    private sealed class TestTenantContext(Guid _tenantId) : ITenantContext
    {
        public Guid TenantId => _tenantId;

        public bool TryGetTenantId(out Guid tenantId)
        {
            tenantId = _tenantId;
            return true;
        }
    }
}
