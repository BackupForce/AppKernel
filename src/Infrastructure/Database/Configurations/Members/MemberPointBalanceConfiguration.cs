using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberPointBalanceConfiguration : IEntityTypeConfiguration<MemberPointBalance>
{
    public void Configure(EntityTypeBuilder<MemberPointBalance> builder)
    {
        builder.ToTable("member_point_balance", Schemas.Default);

        builder.HasKey(b => b.MemberId);
        builder.Ignore(b => b.Id);

        builder.Property(b => b.Balance)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
