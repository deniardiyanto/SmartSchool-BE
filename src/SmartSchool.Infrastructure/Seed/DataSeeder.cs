using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(
        SmartSchoolDbContext context,
        IPasswordHasher passwordHasher)
    {
        // =========================================================
        // 1. SEED ROLES
        // =========================================================

        var adminRole = await context.Roles
            .FirstOrDefaultAsync(x => x.Name == "Admin");

        if (adminRole == null)
        {
            adminRole = new Role
            {
                Name = "Admin",
                Description = "System Administrator",
                IsActive = true
            };

            context.Roles.Add(adminRole);
        }

        var officerRole = await context.Roles
            .FirstOrDefaultAsync(x => x.Name == "Officer");

        if (officerRole == null)
        {
            officerRole = new Role
            {
                Name = "Officer",
                Description = "Petugas Absensi",
                IsActive = true
            };

            context.Roles.Add(officerRole);
        }

        var guardianRole = await context.Roles
            .FirstOrDefaultAsync(x => x.Name == "Guardian");

        if (guardianRole == null)
        {
            guardianRole = new Role
            {
                Name = "Guardian",
                Description = "Wali Murid",
                IsActive = true
            };

            context.Roles.Add(guardianRole);
        }

        await context.SaveChangesAsync();


        // =========================================================
        // 2. SEED ADMIN
        // =========================================================

        var adminUser = await context.Users
            .FirstOrDefaultAsync(x => x.Username == "admin");

        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                FullName = "Administrator",
                RoleId = adminRole.Id,
                IsActive = true
            };

            context.Users.Add(adminUser);

            await context.SaveChangesAsync();
        }


        // =========================================================
        // 3. CREATE USER ACCOUNT FOR EXISTING GUARDIANS
        // =========================================================

       var guardians = await context.Guardians
    .Include(x => x.User)
    .Where(x =>
        x.IsActive &&
        !x.IsDeleted &&
        x.UserId == null)
    .ToListAsync();

foreach (var guardian in guardians)
{
    var username = $"guardian_{guardian.GuardianCode}";

    var existingUser = await context.Users
        .FirstOrDefaultAsync(x => x.Username == username);

    if (existingUser != null)
    {
        guardian.UserId = existingUser.Id;
        continue;
    }

    var guardianUser = new User
    {
        Username = username,
        PasswordHash = passwordHasher.Hash("Admin123!"),
        FullName = guardian.FullName,
        Email = guardian.Email,
        PhoneNumber = guardian.PhoneNumber,
        RoleId = guardianRole.Id,
        IsActive = true
    };

    context.Users.Add(guardianUser);

    await context.SaveChangesAsync();

    guardian.UserId = guardianUser.Id;
}

await context.SaveChangesAsync();
    }
}