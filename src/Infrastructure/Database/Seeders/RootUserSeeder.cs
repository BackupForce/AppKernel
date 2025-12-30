using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Infrastructure;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel.Identity;

namespace Infrastructure.Database.Seeders;
public class RootUserSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;
    private readonly ILogger<RootUserSeeder> _logger;

    public RootUserSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration config,
        ILogger<RootUserSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        string email = _config["RootUser:Email"] ?? RootUser.DefaultEmail;
        string? password = _config["RootUser:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("⚠️ RootUser password not provided. Skipping root seeding.");
            return;
        }

        Email rootEmail = Email.Create(email).Value;

        User? existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == rootEmail);
        if (existing != null)
        {
            _logger.LogInformation("✅ Root user already exists: {Email}", email);
            return;
        }

        var user = User.Create(rootEmail, new Name("root"), _passwordHasher.Hash(password), false);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("🚀 Root user created: {Email}", email);
    }
}
