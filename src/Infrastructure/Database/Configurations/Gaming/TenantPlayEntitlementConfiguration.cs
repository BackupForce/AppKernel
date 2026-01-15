using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TenantPlayEntitlementConfiguration : IEntityTypeConfiguration<TenantPlayEntitlement>
{
    public void Configure(EntityTypeBuilder<TenantPlayEntitlement> builder)
    {
        builder.ToTable("tenant_play_entitlements", Schemas.Gaming);

        builder.HasKey(entitlement => entitlement.Id);

        builder.Property(entitlement => entitlement.TenantId).IsRequired();
        builder.Property(entitlement => entitlement.GameCode)
            .HasConversion(code => code.Value, value => new GameCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(entitlement => entitlement.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(entitlement => entitlement.IsEnabled).IsRequired();
        builder.Property(entitlement => entitlement.EnabledAtUtc).IsRequired();
        builder.Property(entitlement => entitlement.DisabledAtUtc);

        builder.HasIndex(entitlement => new { entitlement.TenantId, entitlement.GameCode, entitlement.PlayTypeCode })
            .IsUnique()
            .HasDatabaseName("ux_tenant_play_entitlements_tenant_game_play");
    }
}
