using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TenantGameEntitlementConfiguration : IEntityTypeConfiguration<TenantGameEntitlement>
{
    public void Configure(EntityTypeBuilder<TenantGameEntitlement> builder)
    {
        builder.ToTable("tenant_game_entitlements", Schemas.Gaming);

        builder.HasKey(entitlement => entitlement.Id);

        builder.Property(entitlement => entitlement.TenantId).IsRequired();
        builder.Property(entitlement => entitlement.GameCode)
            .HasConversion(code => code.Value, value => new GameCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(entitlement => entitlement.IsEnabled).IsRequired();
        builder.Property(entitlement => entitlement.EnabledAtUtc).IsRequired();
        builder.Property(entitlement => entitlement.DisabledAtUtc);

        builder.HasIndex(entitlement => new { entitlement.TenantId, entitlement.GameCode })
            .IsUnique()
            .HasDatabaseName("ux_tenant_game_entitlements_tenant_game");
    }
}
