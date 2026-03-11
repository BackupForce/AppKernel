using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.GetMy;
using Application.IntegrationTests.Infrastructure;
using Domain.Gaming.Catalog;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Draws;
using Domain.Gaming.TicketClaimEvents;
using Domain.Gaming.Tickets;
using Domain.Members;
using Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application.IntegrationTests.Gaming;

public sealed class GetMyTicketsExpiresAtTests : BaseIntegrationTest
{
    public GetMyTicketsExpiresAtTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_Should_ReturnExpiresAtUtc_From_DrawGroup_Or_Draw()
    {
        DateTime now = new DateTime(2024, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        string gameCode = "539";

        (Guid tenantId, Guid userId, Guid memberId) = await SeedMemberAsync(now);

        Guid drawGroupId = Guid.NewGuid();
        DateTime groupCloseAt = now.AddHours(4);
        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.draw_groups (id, tenant_id, game_code, play_type_code, name, grant_open_at_utc, grant_close_at_utc, status, created_at_utc)
            VALUES ({drawGroupId}, {tenantId}, {gameCode}, {"P1"}, {"Group A"}, {now.AddHours(-1)}, {groupCloseAt}, {(int)DrawGroupStatus.Enabled}, {now});
            """);

        Guid manualDrawId = Guid.NewGuid();
        DateTime salesCloseAt = now.AddHours(2);
        DateTime manualCloseAt = now.AddHours(1);
        await InsertDrawAsync(tenantId, manualDrawId, gameCode, now.AddHours(-2), salesCloseAt, now.AddHours(6), manualCloseAt);

        Guid autoDrawId = Guid.NewGuid();
        DateTime autoSalesCloseAt = now.AddHours(3);
        await InsertDrawAsync(tenantId, autoDrawId, gameCode, now.AddHours(-2), autoSalesCloseAt, now.AddHours(8), null);

        Guid ticketWithGroup = await InsertTicketAsync(tenantId, memberId, gameCode, now.AddMinutes(-30), drawGroupId);
        await InsertTicketDrawAsync(tenantId, ticketWithGroup, manualDrawId, now.AddMinutes(-20));

        Guid ticketWithManualDraw = await InsertTicketAsync(tenantId, memberId, gameCode, now.AddMinutes(-25), null);
        await InsertTicketDrawAsync(tenantId, ticketWithManualDraw, manualDrawId, now.AddMinutes(-15));

        Guid ticketWithAutoDraw = await InsertTicketAsync(tenantId, memberId, gameCode, now.AddMinutes(-20), null);
        await InsertTicketDrawAsync(tenantId, ticketWithAutoDraw, autoDrawId, now.AddMinutes(-10));

        Guid ticketWithoutDraw = await InsertTicketAsync(tenantId, memberId, gameCode, now.AddMinutes(-10), null);

        await using AsyncServiceScope scope = ServiceProvider.CreateAsyncScope();

        var handler = new GetMyTicketsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>(),
            scope.ServiceProvider.GetRequiredService<IMemberRepository>(),
            new TestTenantContext(tenantId),
            new TestUserContext(userId, tenantId),
            new TestEntitlementChecker());

        Result<PagedResult<TicketSummaryDto>> result = await handler.Handle(
            new GetMyTicketsQuery(gameCode, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        IReadOnlyCollection<TicketSummaryDto> tickets = result.Value.Items;

        tickets.Single(ticket => ticket.TicketId == ticketWithGroup)
            .ExpiresAtUtc.Should().Be(groupCloseAt);
        tickets.Single(ticket => ticket.TicketId == ticketWithManualDraw)
            .ExpiresAtUtc.Should().Be(manualCloseAt);
        tickets.Single(ticket => ticket.TicketId == ticketWithAutoDraw)
            .ExpiresAtUtc.Should().Be(autoSalesCloseAt);
        tickets.Single(ticket => ticket.TicketId == ticketWithoutDraw)
            .ExpiresAtUtc.Should().BeNull();
    }


    [Fact]
    public async Task Handle_Should_Return_ClaimEventName_And_Keep_ItemCount()
    {
        DateTime now = new DateTime(2024, 3, 2, 9, 0, 0, DateTimeKind.Utc);
        string gameCode = "539";

        (Guid tenantId, Guid userId, Guid memberId) = await SeedMemberAsync(now);

        Guid ticketWithClaimEvent = await InsertTicketAsync(tenantId, memberId, gameCode, now.AddMinutes(-30), null);
        Guid ticketWithoutClaimEvent = await InsertTicketAsync(tenantId, memberId, gameCode, now.AddMinutes(-20), null);

        await InsertClaimEventRecordAsync(
            tenantId,
            memberId,
            ticketWithClaimEvent,
            "新手領券活動",
            now.AddMinutes(-10));

        await using AsyncServiceScope scope = ServiceProvider.CreateAsyncScope();

        var handler = new GetMyTicketsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>(),
            scope.ServiceProvider.GetRequiredService<IMemberRepository>(),
            new TestTenantContext(tenantId),
            new TestUserContext(userId, tenantId),
            new TestEntitlementChecker());

        Result<PagedResult<TicketSummaryDto>> result = await handler.Handle(
            new GetMyTicketsQuery(gameCode, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);

        result.Value.Items.Single(item => item.TicketId == ticketWithClaimEvent)
            .ClaimEventName.Should().Be("新手領券活動");

        result.Value.Items.Single(item => item.TicketId == ticketWithoutClaimEvent)
            .ClaimEventName.Should().BeNull();
    }

    private async Task<(Guid tenantId, Guid userId, Guid memberId)> SeedMemberAsync(DateTime now)
    {
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();

        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO public.users (id, normalized_email, password_hash, has_public_profile, type, tenant_id, email, name)
            VALUES ({userId}, {"TEST@EXAMPLE.COM"}, {"hash"}, {false}, {(int)UserType.Member}, {tenantId}, {"test@example.com"}, {"Test User"});
            """);

        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO public.members (id, user_id, tenant_id, member_no, display_name, status, created_at, updated_at)
            VALUES ({memberId}, {userId}, {tenantId}, {"M0001"}, {"Test Member"}, {(short)MemberStatus.Active}, {now}, {now});
            """);

        return (tenantId, userId, memberId);
    }

    private async Task InsertDrawAsync(
        Guid tenantId,
        Guid drawId,
        string gameCode,
        DateTime salesOpenAt,
        DateTime salesCloseAt,
        DateTime drawAt,
        DateTime? manualCloseAt)
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
                {drawId.ToString()},
                {salesCloseAt},
                {drawAt},
                {(int)DrawStatus.SalesOpen},
                NULL,
                NULL,
                NULL,
                NULL,
                NULL,
                {manualCloseAt.HasValue},
                {manualCloseAt},
                NULL,
                NULL,
                NULL,
                NULL,
                {salesOpenAt},
                {salesOpenAt},
                NULL,
                NULL);
            """);
    }

    private async Task<Guid> InsertTicketAsync(
        Guid tenantId,
        Guid memberId,
        string gameCode,
        DateTime issuedAtUtc,
        Guid? drawGroupId)
    {
        Guid ticketId = Guid.NewGuid();

        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.tickets (
                id,
                tenant_id,
                game_code,
                play_type_code,
                member_id,
                draw_group_id,
                ticket_template_id,
                price_snapshot,
                total_cost,
                issued_at_utc,
                issued_by_type,
                issued_by_user_id,
                issued_reason,
                issued_note,
                submission_status,
                submitted_at_utc,
                submitted_by_user_id,
                submitted_client_reference,
                submitted_note,
                cancelled_at_utc,
                cancelled_reason,
                cancelled_by_user_id,
                created_at,
                draw_id)
            VALUES (
                {ticketId},
                {tenantId},
                {gameCode},
                {"P1"},
                {memberId},
                {drawGroupId},
                NULL,
                NULL,
                NULL,
                {issuedAtUtc},
                {(int)IssuedByType.System},
                NULL,
                NULL,
                NULL,
                {(int)TicketSubmissionStatus.NotSubmitted},
                NULL,
                NULL,
                NULL,
                NULL,
                NULL,
                NULL,
                NULL,
                {issuedAtUtc},
                NULL);
            """);

        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.ticket_lines (id, ticket_id, line_index, numbers_raw, play_type_code)
            VALUES ({Guid.NewGuid()}, {ticketId}, {0}, {"01,02,03,04,05"}, {"P1"});
            """);

        return ticketId;
    }


    private async Task InsertClaimEventRecordAsync(
        Guid tenantId,
        Guid memberId,
        Guid ticketId,
        string eventName,
        DateTime now)
    {
        Guid eventId = Guid.NewGuid();
        string ticketIdsJson = $"[\"{ticketId}\"]";
        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.ticket_claim_events (
                id,
                tenant_id,
                name,
                starts_at_utc,
                ends_at_utc,
                status,
                total_quota,
                total_claimed,
                per_member_quota,
                scope_type,
                scope_id,
                ticket_template_id,
                created_at_utc,
                updated_at_utc)
            VALUES (
                {eventId},
                {tenantId},
                {eventName},
                {now.AddDays(-1)},
                {now.AddDays(1)},
                {(int)TicketClaimEventStatus.Active},
                {100},
                {1},
                {1},
                {(int)TicketClaimEventScopeType.SingleDraw},
                {Guid.NewGuid()},
                NULL,
                {now},
                {now});
            """);

        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.ticket_claim_records (
                id,
                tenant_id,
                event_id,
                member_id,
                quantity,
                idempotency_key,
                issued_ticket_ids,
                claimed_at_utc)
            VALUES (
                {Guid.NewGuid()},
                {tenantId},
                {eventId},
                {memberId},
                {1},
                NULL,
                {ticketIdsJson},
                {now});
            """);
    }

    private async Task InsertTicketDrawAsync(Guid tenantId, Guid ticketId, Guid drawId, DateTime createdAtUtc)
    {
        await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO gaming.ticket_draws (
                id,
                tenant_id,
                ticket_id,
                draw_id,
                participation_status,
                created_at_utc)
            VALUES (
                {Guid.NewGuid()},
                {tenantId},
                {ticketId},
                {drawId},
                {(int)TicketDrawParticipationStatus.Pending},
                {createdAtUtc});
            """);
    }

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId => tenantId;

        public bool TryGetTenantId(out Guid tenantId)
        {
            tenantId = TenantId;
            return true;
        }
    }

    private sealed class TestUserContext(Guid userId, Guid tenantId) : IUserContext
    {
        public Guid UserId => userId;

        public Domain.Users.UserType UserType => Domain.Users.UserType.Member;

        public Guid? TenantId => tenantId;
    }

    private sealed class TestEntitlementChecker : IEntitlementChecker
    {
        public Task<Result> EnsureGameEnabledAsync(Guid tenantId, GameCode gameCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result> EnsurePlayEnabledAsync(
            Guid tenantId,
            GameCode gameCode,
            PlayTypeCode playTypeCode,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<TenantEntitlementsDto> GetTenantEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TenantEntitlementsDto(
                Array.Empty<string>(),
                new Dictionary<string, IReadOnlyCollection<string>>()));
        }
    }
}
