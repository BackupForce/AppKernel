using Domain.Users;
using FluentAssertions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.UnitTests.Users;

public class LoginBindingMappingTests
{
    [Fact]
    public async Task LoginBindings_Should_Load_WithUser()
    {
        string databaseName = Guid.NewGuid().ToString("D");
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        await using (ApplicationDbContext setupContext = CreateDbContext(databaseName))
        {
            User user = User.Create(
                Email.Create("member@test.com").Value,
                new Name("Member"),
                "hash",
                false,
                UserType.Member,
                tenantId);

            Result bindResult = user.BindLogin(LoginProvider.Line, "line-user", now);
            bindResult.IsSuccess.Should().BeTrue();

            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();
        }

        await using ApplicationDbContext queryContext = CreateDbContext(databaseName);
        User? loaded = await queryContext.Users
            .Include(u => u.LoginBindings)
            .FirstOrDefaultAsync(u => u.TenantId == tenantId);

        loaded.Should().NotBeNull();
        loaded!.LoginBindings.Should().HaveCount(1);
        loaded.LoginBindings.First().Provider.Should().Be(LoginProvider.Line);
    }

    private static ApplicationDbContext CreateDbContext(string databaseName)
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
