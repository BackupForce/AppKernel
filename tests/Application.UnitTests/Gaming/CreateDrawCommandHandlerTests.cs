using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Draws.Create;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class CreateDrawCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_DrawCode_With_Prefix_And_Sequence()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        DateTime salesStartAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        DateTime salesCloseAt = salesStartAt.AddHours(2);
        DateTime drawAt = salesCloseAt.AddHours(1);

        CreateDrawCommand command = new(
            "539",
            Array.Empty<string>(),
            salesStartAt,
            salesCloseAt,
            drawAt,
            null);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        IDrawSequenceRepository drawSequenceRepository = Substitute.For<IDrawSequenceRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        ILottery539RngService rngService = Substitute.For<ILottery539RngService>();
        IServerSeedStore serverSeedStore = Substitute.For<IServerSeedStore>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        Draw? insertedDraw = null;
        drawRepository.When(repo => repo.Insert(Arg.Any<Draw>()))
            .Do(call => insertedDraw = call.Arg<Draw>());

        drawSequenceRepository.GetNextSequenceAsync(
            tenantId,
            Arg.Any<GameCode>(),
            "2601",
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        entitlementChecker.EnsureGameEnabledAsync(tenantId, Arg.Any<GameCode>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        CreateDrawCommandHandler handler = new(
            drawRepository,
            drawSequenceRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            rngService,
            serverSeedStore,
            entitlementChecker);

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        insertedDraw.Should().NotBeNull();
        insertedDraw!.DrawCode.Should().Be("539-2601001");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Sequence_Exceeded()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        DateTime salesStartAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime salesCloseAt = salesStartAt.AddHours(2);
        DateTime drawAt = salesCloseAt.AddHours(1);

        CreateDrawCommand command = new(
            "539",
            Array.Empty<string>(),
            salesStartAt,
            salesCloseAt,
            drawAt,
            null);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        IDrawSequenceRepository drawSequenceRepository = Substitute.For<IDrawSequenceRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        ILottery539RngService rngService = Substitute.For<ILottery539RngService>();
        IServerSeedStore serverSeedStore = Substitute.For<IServerSeedStore>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawSequenceRepository.GetNextSequenceAsync(
            tenantId,
            Arg.Any<GameCode>(),
            "2601",
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1000));

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        entitlementChecker.EnsureGameEnabledAsync(tenantId, Arg.Any<GameCode>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        CreateDrawCommandHandler handler = new(
            drawRepository,
            drawSequenceRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            rngService,
            serverSeedStore,
            entitlementChecker);

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.DrawSequenceExceeded);
        drawRepository.DidNotReceive().Insert(Arg.Any<Draw>());
    }
}
