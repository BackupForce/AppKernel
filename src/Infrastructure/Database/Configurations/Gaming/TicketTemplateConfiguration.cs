using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketTemplateConfiguration : IEntityTypeConfiguration<TicketTemplate>
{
    public void Configure(EntityTypeBuilder<TicketTemplate> builder)
    {
        builder.ToTable("ticket_templates", Schemas.Gaming);

        builder.HasKey(template => template.Id);

        builder.Property(template => template.TenantId).IsRequired();
        builder.Property(template => template.Code).HasMaxLength(64).IsRequired();
        builder.Property(template => template.Name).HasMaxLength(128).IsRequired();
        builder.Property(template => template.Type).IsRequired();
        builder.Property(template => template.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(template => template.IsActive).IsRequired();
        builder.Property(template => template.ValidFrom);
        builder.Property(template => template.ValidTo);
        builder.Property(template => template.MaxLinesPerTicket).IsRequired();
        builder.Property(template => template.CreatedAt).IsRequired();
        builder.Property(template => template.UpdatedAt).IsRequired();

        builder.HasIndex(template => new { template.TenantId, template.Code })
            .IsUnique();
        // 中文註解：TenantId + Code 唯一性由資料庫索引保證（migration 由外部處理）。
    }
}
