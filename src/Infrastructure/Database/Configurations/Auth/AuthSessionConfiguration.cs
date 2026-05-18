using Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Auth;

internal sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSession>
{
    public void Configure(EntityTypeBuilder<AuthSession> builder)
    {
        builder.ToTable("auth_sessions", Schemas.Default);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(s => s.LastUsedAtUtc)
            .HasColumnName("last_used_at_utc");

        builder.Property(s => s.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(s => s.RevokedAtUtc)
            .HasColumnName("revoked_at_utc");

        builder.Property(s => s.RevokeReason)
            .HasColumnName("revoke_reason")
            .HasMaxLength(200);

        builder.Property(s => s.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(512);

        builder.Property(s => s.Ip)
            .HasColumnName("ip")
            .HasMaxLength(64);

        builder.Property(s => s.DeviceId)
            .HasColumnName("device_id")
            .HasMaxLength(128);

        builder.HasMany(s => s.RefreshTokens)
            .WithOne(t => t.Session)
            .HasForeignKey(t => t.SessionId);

        builder.HasIndex(s => new { s.TenantId, s.UserId })
            .HasDatabaseName("ix_auth_sessions_tenant_user");
    }
}
