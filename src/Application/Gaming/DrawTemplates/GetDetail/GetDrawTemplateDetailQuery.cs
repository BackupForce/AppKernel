using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.DrawTemplates.GetDetail;

public sealed record GetDrawTemplateDetailQuery(Guid TemplateId) : IQuery<DrawTemplateDetailDto>;
