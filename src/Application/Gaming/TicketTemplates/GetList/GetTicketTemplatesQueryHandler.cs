using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Gaming;
using Domain.Gaming.Repositories;
using Domain.Gaming.TicketTemplates;
using SharedKernel;

namespace Application.Gaming.TicketTemplates.GetList;

internal sealed class GetTicketTemplatesQueryHandler(
    ITicketTemplateRepository ticketTemplateRepository,
    ITenantContext tenantContext) : IQueryHandler<GetTicketTemplatesQuery, IReadOnlyCollection<TicketTemplateDto>>
{
    public async Task<Result<IReadOnlyCollection<TicketTemplateDto>>> Handle(
        GetTicketTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TicketTemplate> templates = await ticketTemplateRepository.GetListAsync(
            tenantContext.TenantId,
            request.ActiveOnly,
            cancellationToken);

        List<TicketTemplateDto> result = new List<TicketTemplateDto>();
        foreach (TicketTemplate template in templates)
        {
            result.Add(new TicketTemplateDto(
                template.Id,
                template.Code,
                template.Name,
                template.Type.ToString(),
                template.Price,
                template.IsActive,
                template.ValidFrom,
                template.ValidTo,
                template.MaxLinesPerTicket,
                template.CreatedAt,
                template.UpdatedAt));
        }

        return result;
    }
}
