using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.DrawTemplates.GetList;

public sealed record GetDrawTemplatesQuery(
    string? GameCode,
    bool? IsActive) : IQuery<IReadOnlyCollection<DrawTemplateSummaryDto>>;
