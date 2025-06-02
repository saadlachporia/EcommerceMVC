using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using EcommerceMVC.Models;

namespace EcommerceMVC.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            const string adminRole = "Admin";
            const string adminEmail = "admin@site.com";
            const string adminPassword = "Admin@123";

            // Create Admin Role if it doesn't exist
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        Console.WriteLine($"Role creation error: {error.Description}");
                }
            }

            // Create Admin User if it doesn't exist
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    IsAdmin = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                            Console.WriteLine($"Add to role error: {error.Description}");
                    }
                }
                else
                {
                    foreach (var error in createResult.Errors)
                        Console.WriteLine($"User creation error: {error.Description}");
                }
            }
        }
    }
}
