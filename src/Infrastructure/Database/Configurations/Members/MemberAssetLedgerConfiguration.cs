using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberAssetLedgerConfiguration : IEntityTypeConfiguration<MemberAssetLedger>
{
    public void Configure(EntityTypeBuilder<MemberAssetLedger> builder)
    {
        builder.ToTable("member_asset_ledger", Schemas.Default);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.AssetCode)
            .IsRequired();

        builder.Property(l => l.Type)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(l => l.Amount)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        builder.Property(l => l.BeforeBalance)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        builder.Property(l => l.AfterBalance)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        builder.Property(l => l.ReferenceType);
        builder.Property(l => l.ReferenceId);
        builder.Property(l => l.OperatorUserId);
        builder.Property(l => l.Remark);
        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasIndex(l => new { l.MemberId, l.AssetCode, l.CreatedAt });

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
