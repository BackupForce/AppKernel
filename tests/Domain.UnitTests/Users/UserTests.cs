using Domain.Users;
using FluentAssertions;

namespace Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_Should_CreateUser_WhenNameIsValid()
    {
        // Arrange
        Email email = Email.Create("test@test.com").Value;
        var name = new Name("Full Name");
        Guid tenantId = Guid.NewGuid();

        // Act
        var user = User.Create(email, name, "hashed", true, UserType.Tenant, tenantId);

        // Assert
        user.Should().NotBeNull();
    }

    [Fact]
    public void Create_Should_RaiseDomainEvent_WhenNameIsValid()
    {
        // Arrange
        Email email = Email.Create("test@test.com").Value;
        var name = new Name("Full Name");
        Guid tenantId = Guid.NewGuid();

        // Act
        var user = User.Create(email, name, "hashed", true, UserType.Tenant, tenantId);

        // Assert
        user.DomainEvents
            .Should().ContainSingle()
            .Which
            .Should().BeOfType<UserCreatedDomainEvent>();
    }
}
