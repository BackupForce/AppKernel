namespace Application.Gaming.Dtos;

public sealed record PrizeDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Cost,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
