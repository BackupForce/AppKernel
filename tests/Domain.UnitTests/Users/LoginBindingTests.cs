using Domain.Users;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Users;

public class LoginBindingTests
{
    [Fact]
    public void SyncProfile_ShouldNotOverwrite_WhenValuesAreMissing()
    {
        Result<LoginBinding> bindingResult = LoginBinding.Create(
            LoginProvider.Line,
            "line-user",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);
        LoginBinding binding = bindingResult.Value;

        Uri initialPictureUrl = new Uri("https://example.com/initial.png");
        binding.SyncProfile("Initial Name", initialPictureUrl, "initial@example.com");

        binding.SyncProfile(null, null, null);
        binding.SyncProfile("   ", null, "   ");

        binding.DisplayName.Should().Be("Initial Name");
        binding.PictureUrl.Should().Be(initialPictureUrl);
        binding.Email.Should().Be("initial@example.com");
    }

    [Fact]
    public void SyncProfile_ShouldUpdate_WhenValuesProvided()
    {
        Result<LoginBinding> bindingResult = LoginBinding.Create(
            LoginProvider.Line,
            "line-user",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);
        LoginBinding binding = bindingResult.Value;

        binding.SyncProfile("Initial Name", new Uri("https://example.com/initial.png"), "initial@example.com");

        Uri updatedPictureUrl = new Uri("https://example.com/updated.png");
        binding.SyncProfile("Updated Name", updatedPictureUrl, "updated@example.com");

        binding.DisplayName.Should().Be("Updated Name");
        binding.PictureUrl.Should().Be(updatedPictureUrl);
        binding.Email.Should().Be("updated@example.com");
    }
}
