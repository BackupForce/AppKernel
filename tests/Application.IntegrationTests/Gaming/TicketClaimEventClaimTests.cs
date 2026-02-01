using Application.Abstractions.Authentication;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.TicketClaimEvents.Claim;
using Application.Gaming.Tickets.Services;
using Application.IntegrationTests.Infrastructure;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketClaimEvents;
using Domain.Members;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application.IntegrationTests.Gaming;

public sealed class TicketClaimEventClaimTests : BaseIntegrationTest
{
    public TicketClaimEventClaimTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Claim_In_Window_Should_Succeed()
    {
        DateTime now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        (Guid eventId, Guid memberId) = await SeedSingleDrawEventAsync(tenantId, userId, now, totalQuota: 10, perMemberQuota: 1);

        Result<TicketClaimResult> result = await ExecuteClaimAsync(tenantId, userId, eventId, now);

        result.IsSuccess.Should().BeTrue();
        result.Value.EventId.Should().Be(eventId);
        result.Value.Quantity.Should().Be(1);
        result.Value.TicketIds.Should().HaveCount(1);

        TicketClaimEvent? persisted = await DbContext.TicketClaimEvents.FindAsync(eventId);
        persisted.Should().NotBeNull();
        persisted!.TotalClaimed.Should().Be(1);

        int recordCount = DbContext.TicketClaimRecords.Count(r => r.EventId == eventId && r.MemberId == memberId);
        recordCount.Should().Be(1);
    }

    [Fact]
    public async Task Claim_Over_TotalQuota_Should_Return_SoldOut()
    {
        DateTime now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Guid tenantId = Guid.NewGuid();
        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();

        Guid eventId = (await SeedSingleDrawEventAsync(tenantId, firstUserId, now, totalQuota: 1, perMemberQuota: 1)).eventId;
        await SeedMemberAsync(tenantId, secondUserId, now, "M0002");

        Result<TicketClaimResult> firstClaim = await ExecuteClaimAsync(tenantId, firstUserId, eventId, now);
        firstClaim.IsSuccess.Should().BeTrue();

        Result<TicketClaimResult> secondClaim = await ExecuteClaimAsync(tenantId, secondUserId, eventId, now);
        secondClaim.IsFailure.Should().BeTrue();
        secondClaim.Error.Should().Be(GamingErrors.TicketClaimEventSoldOut);
    }

    [Fact]
    public async Task Claim_Over_PerMemberQuota_Should_Return_Error()
    {
        DateTime now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        Guid eventId = (await SeedSingleDrawEventAsync(tenantId, userId, now, totalQuota: 2, perMemberQuota: 1)).eventId;

        Result<TicketClaimResult> firstClaim = await ExecuteClaimAsync(tenantId, userId, eventId, now);
        firstClaim.IsSuccess.Should().BeTrue();

        Result<TicketClaimResult> secondClaim = await ExecuteClaimAsync(tenantId, userId, eventId, now);
        secondClaim.IsFailure.Should().BeTrue();
        secondClaim.Error.Should().Be(GamingErrors.TicketClaimEventMemberQuotaExceeded);
    }

    [Fact]
    public async Task Concurrent_Claims_Should_Not_Exceed_TotalQuota()
    {
        DateTime now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Guid tenantId = Guid.NewGuid();
        Guid seedUserId = Guid.NewGuid();

        Guid eventId = (await SeedSingleDrawEventAsync(tenantId, seedUserId, now, totalQuota: 5, perMemberQuota: 1)).eventId;

        List<Guid> userIds = new();
        for (int index = 0; index < 20; index++)
        {
            Guid userId = Guid.NewGuid();
            userIds.Add(userId);
            await SeedMemberAsync(tenantId, userId, now, $"M{index + 10:D4}");
        }

        Task<Result<TicketClaimResult>>[] tasks = userIds
            .Select(userId => ExecuteClaimAsync(tenantId, userId, eventId, now))
            .ToArray();

        Result<TicketClaimResult>[] results = await Task.WhenAll(tasks);

        int successCount = results.Count(result => result.IsSuccess);
        successCount.Should().Be(5);

        TicketClaimEvent? updated = await DbContext.TicketClaimEvents.FindAsync(eventId);
        updated.Should().NotBeNull();
        updated!.TotalClaimed.Should().Be(5);
    }

    private async Task<(Guid eventId, Guid memberId)> SeedSingleDrawEventAsync(
        Guid tenantId,
        Guid userId,
        DateTime now,
        int totalQuota,
        int perMemberQuota)
    {
        Draw draw = Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-TEST",
            now.AddMinutes(-30),
            now.AddMinutes(30),
            now.AddHours(2),
            null,
            now,
            PlayRuleRegistry.CreateDefault()).Value;

        Member member = (await SeedMemberAsync(tenantId, userId, now, "M0001")).member;

        TicketClaimEvent ticketClaimEvent = TicketClaimEvent.Create(
            tenantId,
            "搶票活動",
            now.AddMinutes(-10),
            now.AddMinutes(10),
            totalQuota,
            perMemberQuota,
            TicketClaimEventScopeType.SingleDraw,
            draw.Id,
            null,
            now).Value;

        Result activationResult = ticketClaimEvent.Activate(now);
        activationResult.IsSuccess.Should().BeTrue();

        await EnsureTicketClaimEventTablesAsync();

        DbContext.Draws.Add(draw);
        DbContext.TicketClaimEvents.Add(ticketClaimEvent);
        await DbContext.SaveChangesAsync();

        return (ticketClaimEvent.Id, member.Id);
    }

    private async Task<(Member member, Guid memberId)> SeedMemberAsync(
        Guid tenantId,
        Guid userId,
        DateTime now,
        string memberNo)
    {
        Member member = Member.Create(
            tenantId,
            userId,
            memberNo,
            $"Member {memberNo}",
            now).Value;

        DbContext.Members.Add(member);
        await DbContext.SaveChangesAsync();

        return (member, member.Id);
    }

    private async Task<Result<TicketClaimResult>> ExecuteClaimAsync(
        Guid tenantId,
        Guid userId,
        Guid eventId,
        DateTime now)
    {
        await using AsyncServiceScope scope = ServiceProvider.CreateAsyncScope();

        var ticketClaimEventRepository = scope.ServiceProvider.GetRequiredService<ITicketClaimEventRepository>();
        var ticketClaimMemberCounterRepository = scope.ServiceProvider.GetRequiredService<ITicketClaimMemberCounterRepository>();
        var ticketClaimRecordRepository = scope.ServiceProvider.GetRequiredService<ITicketClaimRecordRepository>();
        var drawGroupRepository = scope.ServiceProvider.GetRequiredService<IDrawGroupRepository>();
        var drawGroupDrawRepository = scope.ServiceProvider.GetRequiredService<IDrawGroupDrawRepository>();
        var drawRepository = scope.ServiceProvider.GetRequiredService<IDrawRepository>();
        var ticketTemplateRepository = scope.ServiceProvider.GetRequiredService<ITicketTemplateRepository>();
        var memberRepository = scope.ServiceProvider.GetRequiredService<IMemberRepository>();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var ticketDrawRepository = scope.ServiceProvider.GetRequiredService<ITicketDrawRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        TicketIssuanceService ticketIssuanceService = new(ticketRepository, ticketDrawRepository);

        ClaimTicketFromEventCommandHandler handler = new(
            ticketClaimEventRepository,
            ticketClaimMemberCounterRepository,
            ticketClaimRecordRepository,
            drawGroupRepository,
            drawGroupDrawRepository,
            drawRepository,
            ticketTemplateRepository,
            memberRepository,
            ticketIssuanceService,
            unitOfWork,
            new FixedDateTimeProvider(now),
            new TestTenantContext(tenantId),
            new TestUserContext(userId, tenantId),
            new TestEntitlementChecker());

        return await handler.Handle(new ClaimTicketFromEventCommand(eventId, null), CancellationToken.None);
    }

    private async Task EnsureTicketClaimEventTablesAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS gaming;");

        await DbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS gaming.ticket_claim_events (
                id uuid PRIMARY KEY,
                tenant_id uuid NOT NULL,
                name varchar(128) NOT NULL,
                starts_at_utc timestamptz NOT NULL,
                ends_at_utc timestamptz NOT NULL,
                status integer NOT NULL,
                total_quota integer NOT NULL,
                total_claimed integer NOT NULL,
                per_member_quota integer NOT NULL,
                scope_type integer NOT NULL,
                scope_id uuid NOT NULL,
                ticket_template_id uuid NULL,
                created_at_utc timestamptz NOT NULL,
                updated_at_utc timestamptz NOT NULL
            );
            """);

        await DbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS gaming.ticket_claim_member_counters (
                event_id uuid NOT NULL,
                member_id uuid NOT NULL,
                claimed_count integer NOT NULL,
                updated_at_utc timestamptz NOT NULL,
                PRIMARY KEY (event_id, member_id)
            );
            """);

        await DbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS gaming.ticket_claim_records (
                id uuid PRIMARY KEY,
                tenant_id uuid NOT NULL,
                event_id uuid NOT NULL,
                member_id uuid NOT NULL,
                quantity integer NOT NULL,
                idempotency_key varchar(64) NULL,
                issued_ticket_ids jsonb NULL,
                claimed_at_utc timestamptz NOT NULL
            );
            """);
    }

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow => utcNow;
    }

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId => tenantId;

        public bool TryGetTenantId(out Guid resolved)
        {
            resolved = tenantId;
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
