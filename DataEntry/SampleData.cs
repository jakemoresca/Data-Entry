using DataEntry.Dao;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataEntry
{
    public class SampleData
    { 

        public async Task InitializeAsync(DataEntryDBContext context)
        {
            //var context = (DataEntryDBContext)serviceProvider.GetService(typeof(DataEntryDBContext));

            string[] roles = new string[] { "Administrator", "User" };

            foreach (string role in roles)
            {
                var roleStore = new RoleStore<IdentityRole>(context);

                if (!context.Roles.Any(r => r.Name == role))
                {
                    var identityRole = new IdentityRole(role);
                    identityRole.NormalizedName = role;
                    await roleStore.CreateAsync(identityRole);
                    await roleStore.AddClaimAsync(identityRole, new Claim("role", "Administrator"));
                }
            }

            var user = new ApplicationUser
            {
                Email = "test@test.com",
                NormalizedEmail = "test@test.COM",
                UserName = "test",
                NormalizedUserName = "test",
                PhoneNumber = "+923366633352",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };


            if (!context.Users.Any(u => u.UserName == user.UserName))
            {
                var password = new PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(user, "test");
                user.PasswordHash = hashed;

                var userStore = new UserStore<ApplicationUser>(context);
                var result = userStore.CreateAsync(user);

                var claims = new List<Claim>
                {
                    new Claim("email", user.Email),
                    //new Claim("role", "Administrator"),
                    new Claim("name", user.UserName)
                };

                await userStore.AddClaimsAsync(user, claims);
                await userStore.AddToRoleAsync(user, "Administrator");
            }

            //var roleStore = new RoleStore<IdentityRole<string>>(context);

            //await userManager.AddClaimAsync(user, new Claim("email", user.Email));
            //await userManager.AddClaimAsync(user, new Claim("role", "admin"));
            //await AssignRoles(userManager, user.Email, roles);
            //await AssignClaims(userManager, user.Email, roles);

            await context.SaveChangesAsync();
        }
    }
}
