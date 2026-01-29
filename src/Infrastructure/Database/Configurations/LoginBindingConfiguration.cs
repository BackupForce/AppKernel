using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class LoginBindingConfiguration : IEntityTypeConfiguration<LoginBinding>
{
    public void Configure(EntityTypeBuilder<LoginBinding> builder)
    {
        builder.ToTable("user_login_bindings", Schemas.Default);

        builder.HasKey(binding => binding.Id);

        builder.Property(binding => binding.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(binding => binding.TenantId)
            .HasColumnName("tenant_id");

        builder.Property(binding => binding.Provider)
            .HasColumnName("provider")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(binding => binding.ProviderKey)
            .HasColumnName("provider_key")
            .IsRequired();

        builder.Property(binding => binding.NormalizedProviderKey)
            .HasColumnName("normalized_provider_key")
            .IsRequired();

        builder.Property(binding => binding.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(binding => binding.DisplayName)
            .HasColumnName("display_name");

        builder.Property(binding => binding.PictureUrl)
            .HasColumnName("picture_url");

        builder.Property(binding => binding.Email)
            .HasColumnName("email");

        builder.HasIndex(binding => new { binding.TenantId, binding.Provider, binding.NormalizedProviderKey })
            .IsUnique()
            .HasDatabaseName("ux_login_bindings_tenant_provider_key")
            .HasFilter("tenant_id IS NOT NULL");

        builder.HasIndex(binding => new { binding.UserId, binding.Provider })
            .IsUnique()
            .HasDatabaseName("ux_login_bindings_user_provider");
    }
}
