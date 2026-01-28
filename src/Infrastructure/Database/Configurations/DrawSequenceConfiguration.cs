using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;
public sealed class DrawSequenceConfiguration : IEntityTypeConfiguration<DrawSequence>
{
    public void Configure(EntityTypeBuilder<DrawSequence> builder)
    {
        builder.ToTable("draw_sequences", "gaming", t =>
        {
            t.HasCheckConstraint("ck_draw_sequences_next_value", "next_value >= 1");
        });


        builder.HasKey(x => new { x.TenantId, x.GameCode });


        builder.Property(x => x.TenantId)
        .HasColumnName("tenant_id")
        .IsRequired();


        builder.Property(x => x.GameCode)
        .HasColumnName("game_code")
        .HasMaxLength(32)
        .IsRequired();


        builder.Property(x => x.NextValue)
        .HasColumnName("next_value")
        .IsRequired();


        builder.Property(x => x.UpdatedAtUtc)
        .HasColumnName("updated_at_utc")
        .IsRequired();
    }
}
