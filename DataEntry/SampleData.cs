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

        public async static void Initialize(DataEntryDBContext context, UserManager<ApplicationUser> userManager)
        {
            //var context = (DataEntryDBContext)serviceProvider.GetService(typeof(DataEntryDBContext));

            string[] roles = new string[] { "Owner", "Administrator", "Manager", "Editor", "Buyer", "Business", "Seller", "Subscriber" };

            //foreach (string role in roles)
            //{
            //    var roleStore = new RoleStore<IdentityRole>(context);

            //    if (!context.Roles.Any(r => r.Name == role))
            //    {
            //        await roleStore.CreateAsync(new IdentityRole(role));
            //    }
            //}


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

            }

            await userManager.AddClaimAsync(user, new Claim("email", user.Email));
            await userManager.AddClaimAsync(user, new Claim("role", "admin"));
            //await AssignRoles(userManager, user.Email, roles);
            //await AssignClaims(userManager, user.Email, roles);

            await context.SaveChangesAsync();
        }

        public static async Task<IdentityResult> AssignRoles(UserManager<ApplicationUser> userManager, string email, string[] roles)
        {
            ApplicationUser user = await userManager.FindByEmailAsync(email);
            var result = await userManager.AddToRolesAsync(user, roles);
            return result;
        }

        public static async Task<IdentityResult> AssignClaims(UserManager<ApplicationUser> userManager, string email, string[] roles)
        {
            ApplicationUser user = await userManager.FindByEmailAsync(email);
            var result = await userManager.AddClaimAsync(user, new Claim("email", email));
            result = await userManager.AddClaimAsync(user, new Claim("role", "admin"));
            return result;
        }

    }
}
