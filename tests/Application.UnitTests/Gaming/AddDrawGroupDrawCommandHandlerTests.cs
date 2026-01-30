using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Time;
using Application.Gaming.DrawGroups.Draws.Add;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Catalog;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class AddDrawGroupDrawCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Reject_When_DrawGroup_Not_Draft()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        DrawGroup drawGroup = DrawGroup.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "非草稿活動",
            now.AddHours(-2),
            now.AddHours(2),
            DrawGroupStatus.Active,
            now).Value;

        IDrawGroupRepository drawGroupRepository = Substitute.For<IDrawGroupRepository>();
        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();

        drawGroupRepository.GetByIdAsync(tenantId, drawGroup.Id, Arg.Any<CancellationToken>())
            .Returns(drawGroup);
        tenantContext.TenantId.Returns(tenantId);

        AddDrawGroupDrawCommandHandler handler = new(
            drawGroupRepository,
            drawRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext);

        Result result = await handler.Handle(
            new AddDrawGroupDrawCommand(tenantId, drawGroup.Id, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.DrawGroupNotDraft);
        await drawRepository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(Guid.Empty, Guid.Empty, default);
    }
}
