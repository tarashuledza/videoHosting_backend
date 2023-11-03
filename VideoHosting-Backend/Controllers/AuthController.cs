using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;
using Web_API.Models.ProfileModels;
using static VideoHosting_Backend.Controllers.AuthController;
using Response = Web_API.Models.ProfileModels.Response;
using Microsoft.AspNetCore.Authorization;

namespace VideoHosting_Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : Controller
    {

        public record SignInRequest(string Email, string Password);
        public record Response(bool IsSuccess, string Message);
        public record UserClaim(string Type, string Value);
        public record User(string Id, string Email, string FirstName, string LastName); // Include isAdmin property in User record

        private readonly StoreDbContext dbContext;
        public AuthController(StoreDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInRequest signInRequest)
        {
            var user = dbContext.Auth.FirstOrDefault(x => x.Email == signInRequest.Email &&
                                                x.Password == signInRequest.Password);
            if (user is null)
            {
                return BadRequest(new Response(false, "Invalid credentials."));
            }

            var claims = new List<Claim>
            {
                new Claim(type: ClaimTypes.Email, value: signInRequest.Email),
                new Claim(type: ClaimTypes.Name, value: user.FirstName),
                new Claim(type: ClaimTypes.Surname, value: user.LastName),
                new Claim("Id", user.Id.ToString()), // Convert the unique identifier to a string
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            return Ok(new Response(true, "Signed in successfully"));
        }
        [Authorize]
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            var userClaims = HttpContext.User.Claims.ToList();

            var emailClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var firstNameClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var lastNameClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            var userIDClaim = userClaims.FirstOrDefault(c => c.Type == "Id");

            if (userIDClaim == null)
                return Unauthorized(new { isSuccess = false, message = "You are not logged in" });

            var email = emailClaim?.Value;
            var firstName = firstNameClaim?.Value;
            var lastName = lastNameClaim?.Value;
            var userID = userIDClaim.Value;

            var user = new User(userID, email, firstName, lastName);
            var userClaimsList = userClaims.Select(x => new UserClaim(x.Type, x.Value)).ToList();
            return Ok(new { isSuccess = true, user });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { isSuccess = true, message = "Everything went right" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegistrationRequest registrationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(false, "Invalid registration data."));
            }

            var existingUser = await dbContext.Auth.FirstOrDefaultAsync(x => x.Email == registrationRequest.Email);
            if (existingUser != null)
            {
                return BadRequest(new Response(false, "User with this email alreawedy exists."));
            }

            var newUser = new Auth
            {
                Email = registrationRequest.Email,
                FirstName = registrationRequest.FirstName,
                LastName = registrationRequest.LastName,
                Password = registrationRequest.Password,
            };

            // Assuming your Auth model represents the user entity
            dbContext.Auth.Add(newUser);
            await dbContext.SaveChangesAsync();

            return Ok(new Response(true, "User registered successfully."));
        }
    }
}
