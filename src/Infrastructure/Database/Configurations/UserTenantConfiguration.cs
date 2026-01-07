using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class UserTenantConfiguration : IEntityTypeConfiguration<UserTenant>
{
    public void Configure(EntityTypeBuilder<UserTenant> builder)
    {
        builder.ToTable("user_tenants", Schemas.Default);

        builder.HasKey(userTenant => userTenant.Id);

        builder.Property(userTenant => userTenant.UserId)
            .IsRequired();

        builder.Property(userTenant => userTenant.TenantId)
            .IsRequired();

        builder.HasIndex(userTenant => new { userTenant.UserId, userTenant.TenantId })
            .IsUnique();

        builder.HasOne(userTenant => userTenant.User)
            .WithMany(user => user.UserTenants)
            .HasForeignKey(userTenant => userTenant.UserId);
    }
}
