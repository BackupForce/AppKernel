using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class DrawTemplateRepository(ApplicationDbContext context) : IDrawTemplateRepository
{
    public async Task<DrawTemplate?> GetByIdAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default)
    {
        return await context.DrawTemplates
            .Include(template => template.PlayTypes)
            .Include(template => template.PrizeTiers)
            .Include(template => template.AllowedTicketTemplates)
            .FirstOrDefaultAsync(
                template => template.TenantId == tenantId && template.Id == templateId,
                cancellationToken);
    }

    public async Task<DrawTemplate?> GetByNameAsync(
        Guid tenantId,
        string gameCode,
        string name,
        CancellationToken cancellationToken = default)
    {
        return await context.DrawTemplates
            .FirstOrDefaultAsync(
                template => template.TenantId == tenantId
                            && template.GameCode.Value == gameCode
                            && template.Name == name,
                cancellationToken);
    }

    public async Task<bool> HasDrawsAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default)
    {
        return await context.Draws.AnyAsync(
            draw => draw.TenantId == tenantId && draw.SourceTemplateId == templateId,
            cancellationToken);
    }

    public void Insert(DrawTemplate template)
    {
        context.DrawTemplates.Add(template);
    }

    public void Update(DrawTemplate template)
    {
        context.DrawTemplates.Update(template);
    }
}
