using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin.Winnings;
using Application.Abstractions.Gaming;
using Domain.Gaming.Shared;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class AdminRedeemWinningCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Winning_Already_Redeemed()
    {
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid winningId = Guid.NewGuid();

        IAdminWinningRedemptionRepository repository = Substitute.For<IAdminWinningRedemptionRepository>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(userId);
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc));

        repository.RedeemAsync(tenantId, winningId, userId, dateTimeProvider.UtcNow, Arg.Any<CancellationToken>())
            .Returns(AdminWinningRedemptionResult.Conflict);

        RedeemWinningCommandHandler handler = new RedeemWinningCommandHandler(
            repository,
            tenantContext,
            userContext,
            dateTimeProvider);

        Result<AdminWinningDetailDto> result = await handler.Handle(new RedeemWinningCommand(winningId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.TicketLineResultAlreadyRedeemed);
    }

    [Fact]
    public async Task Handle_Should_Return_Detail_When_Redeem_Success()
    {
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid winningId = Guid.NewGuid();

        IAdminWinningRedemptionRepository repository = Substitute.For<IAdminWinningRedemptionRepository>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        AdminWinningDetailDto detail = new AdminWinningDetailDto
        {
            WinningId = winningId,
            TenantId = tenantId,
            TicketId = Guid.NewGuid(),
            DrawId = Guid.NewGuid(),
            DrawDateUtc = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            DrawCode = "539-260301",
            PayoutAmount = 1000,
            PrizeTier = "A",
            SettledAtUtc = new DateTime(2026, 3, 1, 1, 0, 0, DateTimeKind.Utc)
        };

        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(userId);
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc));

        repository.RedeemAsync(tenantId, winningId, userId, dateTimeProvider.UtcNow, Arg.Any<CancellationToken>())
            .Returns(AdminWinningRedemptionResult.Success);
        repository.GetWinningDetailAsync(tenantId, winningId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(detail));

        RedeemWinningCommandHandler handler = new RedeemWinningCommandHandler(
            repository,
            tenantContext,
            userContext,
            dateTimeProvider);

        Result<AdminWinningDetailDto> result = await handler.Handle(new RedeemWinningCommand(winningId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.WinningId.Should().Be(winningId);
    }
}
