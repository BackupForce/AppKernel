using Domain.Tenants;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", Schemas.Default);

        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Code)
            .IsRequired();

        builder.Property(tenant => tenant.Name)
            .IsRequired();

        builder.Property(tenant => tenant.TimeZoneId)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(tenant => tenant.Code)
            .IsUnique();
    }
}
