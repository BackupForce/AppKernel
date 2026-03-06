using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberTagConfiguration : IEntityTypeConfiguration<MemberTag>
{
    public void Configure(EntityTypeBuilder<MemberTag> builder)
    {
        builder.ToTable("member_tags_catalog", Schemas.Default);

        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.TenantId).IsRequired();
        builder.Property(tag => tag.TagCode).HasMaxLength(64).IsRequired();
        builder.Property(tag => tag.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(tag => tag.IsActive).IsRequired();
        builder.Property(tag => tag.CreatedAtUtc).IsRequired();
        builder.Property(tag => tag.UpdatedAtUtc).IsRequired();

        builder.HasIndex(tag => new { tag.TenantId, tag.TagCode })
            .IsUnique()
            .HasDatabaseName("ux_member_tags_catalog_tenant_id_tag_code");

        builder.HasIndex(tag => new { tag.TenantId, tag.IsActive })
            .HasDatabaseName("ix_member_tags_catalog_tenant_id_is_active");
    }
}
