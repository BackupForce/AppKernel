using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberPointLedgerConfiguration : IEntityTypeConfiguration<MemberPointLedger>
{
    public void Configure(EntityTypeBuilder<MemberPointLedger> builder)
    {
        builder.ToTable("member_point_ledger", Schemas.Default);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Type)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(l => l.Amount)
            .IsRequired();

        builder.Property(l => l.BeforeBalance)
            .IsRequired();

        builder.Property(l => l.AfterBalance)
            .IsRequired();

        builder.Property(l => l.ReferenceType);
        builder.Property(l => l.ReferenceId);
        builder.Property(l => l.OperatorUserId);
        builder.Property(l => l.Remark);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.HasIndex(l => new { l.MemberId, l.CreatedAt });
        builder.HasIndex(l => new { l.ReferenceType, l.ReferenceId });

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
