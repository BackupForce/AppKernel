using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.ComplexProperty(
            u => u.Email,
            b => b.Property(e => e.Value).HasColumnName("email"));

        builder.ComplexProperty(
            u => u.Name,
            b => b.Property(e => e.Value).HasColumnName("name"));

        builder.Property(u => u.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id");

        builder.HasIndex(u => u.TenantId);

        builder.HasCheckConstraint(
            "ck_users_type_tenant_id",
            "(\"type\" = 0 AND tenant_id IS NULL) OR (\"type\" <> 0 AND tenant_id IS NOT NULL)");
    }
}
