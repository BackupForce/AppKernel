using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberActivityLogConfiguration : IEntityTypeConfiguration<MemberActivityLog>
{
    public void Configure(EntityTypeBuilder<MemberActivityLog> builder)
    {
        builder.ToTable("member_activity_log", Schemas.Default);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Action)
            .IsRequired();

        builder.Property(l => l.Payload);
        builder.Property(l => l.Ip);
        builder.Property(l => l.UserAgent);
        builder.Property(l => l.OperatorUserId);
        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasIndex(l => new { l.MemberId, l.CreatedAt });
        builder.HasIndex(l => new { l.Action, l.CreatedAt });

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
