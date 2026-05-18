using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Gaming.Draws;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawAllowedTicketTemplateConfiguration : IEntityTypeConfiguration<DrawAllowedTicketTemplate>
{
    public void Configure(EntityTypeBuilder<DrawAllowedTicketTemplate> builder)
    {
        builder.ToTable("draw_allowed_ticket_templates", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.DrawId).IsRequired();
        builder.Property(item => item.TicketTemplateId).IsRequired();
        builder.Property(item => item.CreatedAt).IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.DrawId, item.TicketTemplateId })
            .IsUnique();
        // 中文註解：同一 Draw 同一模板只能出現一次，避免重複允許。
    }
}
