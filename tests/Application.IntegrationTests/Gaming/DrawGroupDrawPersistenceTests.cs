using Application.IntegrationTests.Infrastructure;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Catalog;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.IntegrationTests.Gaming;

public sealed class DrawGroupDrawPersistenceTests : BaseIntegrationTest
{
    public DrawGroupDrawPersistenceTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddDraw_Should_Persist_DrawGroupDraw()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        DrawGroup drawGroup = DrawGroup.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "持久化測試活動",
            now.AddHours(1),
            now.AddHours(2),
            DrawGroupStatus.Draft,
            now).Value;

        DbContext.DrawGroups.Add(drawGroup);
        await DbContext.SaveChangesAsync();

        Result addResult = drawGroup.AddDraw(Guid.NewGuid(), now);
        addResult.IsSuccess.Should().BeTrue();

        await DbContext.SaveChangesAsync();

        DrawGroupDraw? persisted = await DbContext.DrawGroupDraws
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.DrawGroupId == drawGroup.Id);

        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveDrawGroup_Should_Cascade_Delete_Draws()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        DrawGroup drawGroup = DrawGroup.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "刪除測試活動",
            now.AddHours(1),
            now.AddHours(2),
            DrawGroupStatus.Draft,
            now).Value;

        DbContext.DrawGroups.Add(drawGroup);
        await DbContext.SaveChangesAsync();

        Result addResult = drawGroup.AddDraw(Guid.NewGuid(), now);
        addResult.IsSuccess.Should().BeTrue();
        await DbContext.SaveChangesAsync();

        DbContext.DrawGroups.Remove(drawGroup);
        await DbContext.SaveChangesAsync();

        int remaining = await DbContext.DrawGroupDraws
            .CountAsync(item => item.TenantId == tenantId && item.DrawGroupId == drawGroup.Id);

        remaining.Should().Be(0);
    }
}
