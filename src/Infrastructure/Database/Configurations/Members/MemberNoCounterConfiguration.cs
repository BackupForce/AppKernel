using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

public sealed class MemberNoCounterConfiguration : IEntityTypeConfiguration<MemberNoCounter>
{
    public void Configure(EntityTypeBuilder<MemberNoCounter> builder)
    {
        builder.ToTable("member_no_counters", "public");

        builder.HasKey(x => x.TenantId);

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.LastValue)
            .HasColumnName("last_value")
            .IsRequired();
    }
}
