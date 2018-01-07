using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEntry.Dao;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DataEntry.Controllers
{
    public class TokenRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AccessToken
    {
        public string Token { get; set; }
        public string Expiration { get; set; }
    }

    [Route("connect/token")]
    public class TokenController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;

        public TokenController(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Post(TokenRequest model)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest("Username and password must not be empty!");
            }

            // Authenticates username and password to your SQL Server database, for example.
            // If authentication is successful, return a user's claims.
            bool isValid = false;
            var user = await _userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                isValid = false;
            }
            else
            {
                isValid = await _userManager.CheckPasswordAsync(user, model.Password);
            }

            if (!isValid)
            {
                return BadRequest("Invalid username or password!");
            }

            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToDictionary(x => x.Type, x => (object)x.Value);

            DateTime centuryBegin = new DateTime(1970, 1, 1);
            var exp = new TimeSpan(DateTime.Now.AddYears(1).Ticks - centuryBegin.Ticks).TotalSeconds;
            var now = DateTime.Now.Ticks;

            claims.Add("iss", "dataentry.com");
            claims.Add("aud", "dataentry.com");
            claims.Add("sub", model.Username);
            claims.Add("iat", now);
            claims.Add("nbf", now);
            claims.Add("exp", exp);

            var tokenSecretKey = Encoding.UTF8.GetBytes("superlongsecrettokenkeyhere");

            // As an example, AuthService.CreateToken can return Jose.JWT.Encode(claims, YourTokenSecretKey, Jose.JwsAlgorithm.HS256);
            var token = Jose.JWT.Encode(claims, tokenSecretKey, Jose.JwsAlgorithm.HS256); //AuthService.CreateToken(claims);

            var accessToken = new AccessToken
            {
                Token = token
            };

            return Ok(accessToken);
        }
    }
}
