using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataEntry.Dao;

namespace DataEntry.Controllers
{
    [Route("api/[controller]")]
    public class MeController : Controller
    {
        private readonly DataEntryDBContext _context;

        public MeController(DataEntryDBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("[action]")]
        public ApplicationUser Get()
        {
            var userClaims = User.Claims.ToList();
            var nameClaim = userClaims.FirstOrDefault(x => x.Type == "name");
            var user = _context.Users.FirstOrDefault(x => x.UserName == nameClaim.Value);

            return user;
        }
    }
}
