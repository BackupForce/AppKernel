using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members", Schemas.Default);

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MemberNo)
            .IsRequired();

        builder.Property(m => m.DisplayName)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.HasIndex(m => m.MemberNo).IsUnique();
        builder.HasIndex(m => m.UserId).IsUnique().HasFilter("user_id IS NOT NULL");

        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(m => m.UserId);
    }
}
