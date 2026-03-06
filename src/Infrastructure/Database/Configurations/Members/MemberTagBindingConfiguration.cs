using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberTagBindingConfiguration : IEntityTypeConfiguration<MemberTagBinding>
{
    public void Configure(EntityTypeBuilder<MemberTagBinding> builder)
    {
        builder.ToTable("member_tag_bindings", Schemas.Default);

        builder.HasKey(binding => binding.Id);

        builder.Property(binding => binding.TenantId).IsRequired();
        builder.Property(binding => binding.MemberId).IsRequired();
        builder.Property(binding => binding.TagId).IsRequired();
        builder.Property(binding => binding.CreatedAtUtc).IsRequired();
        builder.Property(binding => binding.CreatedByUserId).IsRequired(false);

        builder.HasIndex(binding => new { binding.TenantId, binding.MemberId, binding.TagId })
            .IsUnique()
            .HasDatabaseName("ux_member_tag_bindings_tenant_member_tag");

        builder.HasIndex(binding => new { binding.TenantId, binding.MemberId })
            .HasDatabaseName("ix_member_tag_bindings_tenant_member");

        builder.HasIndex(binding => new { binding.TenantId, binding.TagId })
            .HasDatabaseName("ix_member_tag_bindings_tenant_tag");
    }
}
