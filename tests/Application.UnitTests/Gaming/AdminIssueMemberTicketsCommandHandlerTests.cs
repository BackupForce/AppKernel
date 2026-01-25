using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Admin;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Tickets;
using Domain.Members;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Text.Json;

namespace Application.UnitTests.Gaming;

public sealed class AdminIssueMemberTicketsCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Issue_Tickets()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateDraw(tenantId, now.AddHours(-1), now.AddHours(1));
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketIdempotencyRepository ticketIdempotencyRepository = Substitute.For<ITicketIdempotencyRepository>();
        IMemberRepository memberRepository = Substitute.For<IMemberRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        memberRepository.GetByIdAsync(tenantId, memberId, Arg.Any<CancellationToken>())
            .Returns(Member.Create(tenantId, null, "M001", "Tester", now).Value);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, draw.GameCode, draw.EnabledPlayTypes.First(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        IssueMemberTicketsCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            ticketIdempotencyRepository,
            memberRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext
            );

        Result<IssueMemberTicketsResult> result = await handler.Handle(
            new IssueMemberTicketsCommand(
                memberId,
                GameCodes.Lottery539.Value,
                draw.Id,
                2,
                "support",
                "note",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Tickets.Should().HaveCount(2);
        ticketRepository.Received(2).Insert(Arg.Any<Ticket>());
        ticketDrawRepository.Received(2).Insert(Arg.Any<TicketDraw>());
    }

    [Fact]
    public async Task Handle_Should_Return_Idempotent_Response()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        string key = "idem-key";
        Guid drawId = Guid.NewGuid();

        IssueMemberTicketsResult cached = new(
            new[]
            {
                new IssuedTicketDto(Guid.NewGuid(), "Issued", now, Guid.NewGuid(), "LOTTERY539",  Guid.NewGuid(), "support", "note")
            });

        TicketIdempotencyRecord record = TicketIdempotencyRecord.Create(
            tenantId,
            key,
            "issue_ticket",
            ComputeHash(memberId, "LOTTERY539", "BASIC", drawId, 1, "support", "note"),
            JsonSerializer.Serialize(cached, TestJson.Web),
            now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketIdempotencyRepository ticketIdempotencyRepository = Substitute.For<ITicketIdempotencyRepository>();
        IMemberRepository memberRepository = Substitute.For<IMemberRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();

        ticketIdempotencyRepository.GetByKeyAsync(tenantId, key, "issue_ticket", Arg.Any<CancellationToken>())
            .Returns(record);
        tenantContext.TenantId.Returns(tenantId);

        IssueMemberTicketsCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            ticketIdempotencyRepository,
            memberRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext
            );

        Result<IssueMemberTicketsResult> result = await handler.Handle(
            new IssueMemberTicketsCommand(
                memberId,
                "LOTTERY539",
                drawId,
                1,
                "support",
                "note",
                key),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Tickets.Should().HaveCount(1);
        ticketRepository.DidNotReceive().Insert(Arg.Any<Ticket>());
    }

    private static Draw CreateDraw(Guid tenantId, DateTime openAt, DateTime closeAt)
    {
        return Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            openAt,
            closeAt,
            closeAt.AddHours(1),
            DrawStatus.SalesOpen,
            null,
            DateTime.UtcNow,
            PlayRuleRegistry.CreateDefault()).Value;
    }

    private static string ComputeHash(
        Guid memberId,
        string gameCode,
        string playTypeCode,
        Guid drawId,
        int quantity,
        string? reason,
        string? note)
    {
        string raw = $"{memberId:N}|{gameCode}|{playTypeCode}|{drawId:N}|{quantity}|{reason}|{note}";
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)));
    }

    internal static class TestJson
    {
        internal static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    }
}
