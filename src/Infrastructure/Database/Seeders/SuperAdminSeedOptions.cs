namespace Infrastructure.Database.Seeders;

public sealed class SuperAdminSeedOptions
{
    public bool Enabled { get; set; } = true;

    public string RoleName { get; set; } = "SuperAdmin";

    public string UserEmail { get; set; } = "root@system.local";
}
