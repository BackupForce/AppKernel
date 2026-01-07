using Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("user_groups", Schemas.Default);

        builder.HasKey(userGroup => new { userGroup.UserId, userGroup.GroupId });

        builder.HasIndex(userGroup => new { userGroup.UserId, userGroup.GroupId });

        builder.HasOne(userGroup => userGroup.User)
            .WithMany(user => user.UserGroups)
            .HasForeignKey(userGroup => userGroup.UserId);

        builder.HasOne(userGroup => userGroup.Group)
            .WithMany(group => group.UserGroups)
            .HasForeignKey(userGroup => userGroup.GroupId);
    }
}
