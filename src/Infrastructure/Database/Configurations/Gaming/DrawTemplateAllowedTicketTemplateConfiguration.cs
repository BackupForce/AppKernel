using Domain.Gaming.DrawTemplates;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawTemplateAllowedTicketTemplateConfiguration : IEntityTypeConfiguration<DrawTemplateAllowedTicketTemplate>
{
    public void Configure(EntityTypeBuilder<DrawTemplateAllowedTicketTemplate> builder)
    {
        builder.ToTable("draw_template_allowed_ticket_templates", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.TemplateId).IsRequired();
        builder.Property(item => item.TicketTemplateId).IsRequired();
        builder.Property(item => item.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.TemplateId, item.TicketTemplateId })
            .IsUnique();
    }
}
