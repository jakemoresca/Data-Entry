using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataEntry.Dao;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataEntry.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly DataEntryDBContext _context;
        const int ITEMS_PER_PAGE = 10;

        public UserController(DataEntryDBContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("[action]")]
        public IEnumerable<ApplicationUser> Users(int startDateIndex)
        {
            var itemsToSkip = ITEMS_PER_PAGE * startDateIndex;
            return _context.Users
                .Skip(itemsToSkip).Take(ITEMS_PER_PAGE).ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("{userId}")]
        public ApplicationUser Get(string userId)
        {
            return _context.Users
                .FirstOrDefault(x => x.Id == userId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public ApplicationUser Current()
        {
            var userClaims = User.Claims.ToList();
            var nameClaim = userClaims.FirstOrDefault(x => x.Type == "name");
            var user = _context.Users.FirstOrDefault(x => x.UserName == nameClaim.Value);

            return user;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ApplicationUser Post(ApplicationUser user)
        {
            user.NormalizedEmail = user.Email;
            user.NormalizedUserName = user.UserName;
            user.PhoneNumber = "";
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.SecurityStamp = Guid.NewGuid().ToString("D");
            var password = new PasswordHasher<ApplicationUser>();
            var hashed = password.HashPassword(user, user.PasswordHash);
            user.PasswordHash = hashed;

            var userStore = new UserStore<ApplicationUser>(_context);

            var result = userStore.CreateAsync(user);

            var claims = new List<Claim>
            {
                new Claim("email", user.Email),
                new Claim("name", user.UserName)
            };

            userStore.AddClaimsAsync(user, claims);
            userStore.AddToRoleAsync(user, "User");

            _context.SaveChanges();

            return user;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{userId}")]
        public ApplicationUser Put(string userId, ApplicationUser user)
        {
            user.NormalizedEmail = user.Email;
            user.NormalizedUserName = user.UserName;
            user.PhoneNumber = "";
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.SecurityStamp = Guid.NewGuid().ToString("D");
            var password = new PasswordHasher<ApplicationUser>();
            var hashed = password.HashPassword(user, user.PasswordHash);
            user.PasswordHash = hashed;

            var userStore = new UserStore<ApplicationUser>(_context);
            userStore.UpdateAsync(user);

            _context.SaveChanges();

            return user;
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{userId}")]
        public async Task<StatusCodeResult> DeleteAsync(string userId)
        {
            var userStore = new UserStore<ApplicationUser>(_context);
            var user = await userStore.FindByIdAsync(userId);
            await userStore.DeleteAsync(user);

            _context.SaveChanges();

            return Ok();
        }
    }
}
