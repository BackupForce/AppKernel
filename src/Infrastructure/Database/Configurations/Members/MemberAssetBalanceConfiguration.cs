using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberAssetBalanceConfiguration : IEntityTypeConfiguration<MemberAssetBalance>
{
    public void Configure(EntityTypeBuilder<MemberAssetBalance> builder)
    {
        builder.ToTable("member_asset_balance", Schemas.Default);

        builder.HasKey(b => new { b.MemberId, b.AssetCode });
        builder.Ignore(b => b.Id);

        builder.Property(b => b.AssetCode)
            .IsRequired();

        builder.Property(b => b.Balance)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
