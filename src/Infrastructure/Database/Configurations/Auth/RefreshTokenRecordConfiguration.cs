using Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Auth;

internal sealed class RefreshTokenRecordConfiguration : IEntityTypeConfiguration<RefreshTokenRecord>
{
    public void Configure(EntityTypeBuilder<RefreshTokenRecord> builder)
    {
        builder.ToTable("refresh_token_records", Schemas.Default);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(t => t.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(t => t.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(t => t.RevokedAtUtc)
            .HasColumnName("revoked_at_utc");

        builder.Property(t => t.RevokedReason)
            .HasColumnName("revoked_reason")
            .HasMaxLength(200);

        builder.Property(t => t.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id");

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_refresh_token_records_token_hash");

        builder.HasIndex(t => t.SessionId)
            .HasDatabaseName("ix_refresh_token_records_session_id");
    }
}
