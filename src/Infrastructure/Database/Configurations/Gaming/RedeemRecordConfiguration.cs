using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class RedeemRecordConfiguration : IEntityTypeConfiguration<RedeemRecord>
{
    public void Configure(EntityTypeBuilder<RedeemRecord> builder)
    {
        builder.ToTable("Gaming_RedeemRecords");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.TenantId).IsRequired();
        builder.Property(record => record.MemberId).IsRequired();
        builder.Property(record => record.PrizeAwardId).IsRequired();
        builder.Property(record => record.PrizeId).IsRequired();
        builder.Property(record => record.PrizeNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(record => record.CostSnapshot).HasPrecision(18, 2).IsRequired();
        builder.Property(record => record.RedeemedAt).IsRequired();
        builder.Property(record => record.Note).HasMaxLength(256);

        builder.HasIndex(record => record.PrizeAwardId).IsUnique();
    }
}
