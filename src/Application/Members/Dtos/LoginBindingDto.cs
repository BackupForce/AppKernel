using Domain.Users;

namespace Application.Members.Dtos;

public sealed record LoginBindingDto(
    Guid Id,
    LoginProvider Provider,
    string ProviderKey,
    string? DisplayName,
    string? PictureUrl,
    string? Email,
    DateTime CreatedAtUtc);
