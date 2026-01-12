using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawAllowedTicketTemplateConfiguration : IEntityTypeConfiguration<DrawAllowedTicketTemplate>
{
    public void Configure(EntityTypeBuilder<DrawAllowedTicketTemplate> builder)
    {
        builder.ToTable("Gaming_DrawAllowedTicketTemplates");

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
