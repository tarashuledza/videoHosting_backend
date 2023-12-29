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
using VideoHosting_Backend.Models.Auth;
using VideoHosting_Backend.Services;

namespace VideoHosting_Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : Controller
    {

        public record SignInRequest(string Email, string Password);
        public record Response(bool IsSuccess, string Message);
        public record UserClaim(string Type, string Value);
        public record User(string Id, string Email, string FirstName, string LastName, string Thumbnail);
        public record UserById(int Id, string Email, string FirstName, string LastName, bool Followed);

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
        new Claim("Thumbnail", user.Thumbnail) // Convert byte[] thumbnail to base64 string
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



        /*
                [Authorize]
                [HttpGet("user")]
                public IActionResult GetUser()
                {
                    var userClaims = HttpContext.User.Claims.ToList();

                    //var emailClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                    //var firstNameClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                    //var lastNameClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                    var userIDClaim = userClaims.FirstOrDefault(c => c.Type == "Id");
                    var thumbnailClaim = userClaims.FirstOrDefault(c => c.Type == "Thumbnail");

                    if (userIDClaim == null)
                        return Unauthorized(new { isSuccess = false, message = "You are not logged in" });

                    //var email = emailClaim?.Value;
                    //var firstName = firstNameClaim?.Value;
                    //var lastName = lastNameClaim?.Value;
                    var userID = userIDClaim.Value;
                    //var thumbnail = thumbnailClaim?.Value; // Check if thumbnail claim exists

                    var uses = dbContext.Auth.Find(userID);

                    var user = new User(userID, uses.Email, uses.FirstName, uses.LastName, uses.Thumbnail);
                    var userClaimsList = userClaims.Select(x => new UserClaim(x.Type, x.Value)).ToList();
                    return Ok(new { isSuccess = true, user });
                }*/
        [Authorize]
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            var userClaims = HttpContext.User.Claims.ToList();

            var userIDClaim = userClaims.FirstOrDefault(c => c.Type == "Id");

            if (userIDClaim == null)
            {
                return Unauthorized(new { isSuccess = false, message = "You are not logged in" });
            }

            var userID = userIDClaim.Value;

            var user = dbContext.Auth.Find(int.Parse(userID));

            if (user == null)
            {
                return NotFound(new { isSuccess = false, message = "User not found" });
            }

            var responseUser = new User(user.Id.ToString(), user.Email, user.FirstName, user.LastName, user.Thumbnail);

            return Ok(new { isSuccess = true, user = responseUser });
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
                Thumbnail = ""
            };

            dbContext.Auth.Add(newUser);
            await dbContext.SaveChangesAsync();

            return Ok(new Response(true, "User registered successfully."));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await dbContext.Auth.FindAsync(id);
            if (user == null)
            {
                return NotFound(new Response(false, "User not found."));
            }

            var userClaimsList = HttpContext.User.Claims.Select(x => new UserClaim(x.Type, x.Value)).ToList();

            var userIDClaim = userClaimsList.FirstOrDefault(c => c.Type == "Id");
            var emailClaim = userClaimsList.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var firstNameClaim = userClaimsList.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var lastNameClaim = userClaimsList.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
        

            if (userIDClaim == null)
            {
                return Unauthorized(new Response(false, "You are not authorized to view this user."));
            }

            var email = emailClaim?.Value;
            var firstName = firstNameClaim?.Value;
            var lastName = lastNameClaim?.Value;

            int currentUserId = int.Parse(userIDClaim.Value);
            bool isFollowing = dbContext.Followers.Any(f => f.FollowerId == currentUserId && f.UserId == id);

            var userDetails = new { user.Id, user.Email, user.FirstName, user.LastName, user.Thumbnail,user.Subscribers, isFollowing };

            // Check if the current user is following the specified user

            return Ok(userDetails);
        }
        [Authorize]
        [HttpPost("follow/{userIdToFollow}")]
        public async Task<IActionResult> FollowUser(int userIdToFollow)
        {
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null)
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }

            var currentUserId = int.Parse(currentUserIDClaim.Value);

            // Check if the user is trying to follow themselves
            if (userIdToFollow == currentUserId)
            {
                return BadRequest(new Response(false, "You cannot follow yourself."));
            }

            var userToFollow = await dbContext.Auth.FindAsync(userIdToFollow);
            if (userToFollow == null)
            {
                return NotFound(new Response(false, "User to follow not found."));
            }
            userToFollow.Subscribers++;
            // Check if the user is already following the specified user
            var existingRelationship = await dbContext.Followers
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.UserId == userIdToFollow);


            if (existingRelationship != null)
            {
                return BadRequest(new Response(false, "You are already following this user."));
            }

            try
            {
                // Create a new relationship in the Followers table
                var newRelationship = new Followers
                {
                    FollowerId = currentUserId,
                    UserId = userIdToFollow
                };

                dbContext.Followers.Add(newRelationship);
                await dbContext.SaveChangesAsync();

                return Ok(new Response(true, $"You are now following {userToFollow.FirstName} {userToFollow.LastName}."));
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the database operation
                return StatusCode(500, new Response(false, "An error occurred while following the user."));
            }
        }

        [Authorize]
        [HttpDelete("unfollow/{userIdToUnfollow}")]
        public async Task<IActionResult> UnfollowUser(int userIdToUnfollow)
        {
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null)
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }

            var currentUserId = int.Parse(currentUserIDClaim.Value);

            // Check if the user is trying to unfollow themselves
            if (userIdToUnfollow == currentUserId)
            {
                return BadRequest(new Response(false, "You cannot unfollow yourself."));
            }

            var userToUnfollow = await dbContext.Auth.FindAsync(userIdToUnfollow);
            userToUnfollow.Subscribers--;
            if (userToUnfollow == null)
            {
                return NotFound(new Response(false, "User to unfollow not found."));
            }

            var existingRelationship = await dbContext.Followers
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.UserId == userIdToUnfollow);

            if (existingRelationship == null)
            {
                return BadRequest(new Response(false, "You are not currently following this user."));
            }

            try
            {
                // Remove the relationship from the Followers table
                dbContext.Followers.Remove(existingRelationship);
                await dbContext.SaveChangesAsync();

                return Ok(new Response(true, $"You have unfollowed {userToUnfollow.FirstName} {userToUnfollow.LastName}."));
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the database operation
                return StatusCode(500, new Response(false, "An error occurred while unfollowing the user."));
            }
        }
        [Authorize]
        [HttpPatch("updateThumbnail")]
        public async Task<IActionResult> UpdateThumbnailAsync(IFormFile photo)
        {
           
                if (photo == null || photo.Length == 0)
                {
                    return BadRequest(new Response(false, "Photo is required."));
                }

                var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");
                if (currentUserIDClaim == null || !int.TryParse(currentUserIDClaim.Value, out var currentUserId))
                {
                    return Unauthorized(new Response(false, "You are not logged in."));
                }

                var userToUpdate = await dbContext.Auth.FindAsync(currentUserId);
                if (userToUpdate == null)
                {
                    return NotFound(new Response(false, "User not found."));
                }

                // Use a using statement to ensure that resources are properly disposed of
                using (var memoryStream = new MemoryStream())
                {
                    await photo.CopyToAsync(memoryStream);
                    // Convert the byte array to a base64-encoded string
                    userToUpdate.Thumbnail = Convert.ToBase64String(memoryStream.ToArray());
                }
           /* try
            {*/
                // Use Update instead of Add when modifying an existing entity
                dbContext.Update(userToUpdate);
                await dbContext.SaveChangesAsync();

                return Ok(new Response(true, "Thumbnail updated successfully."));
           /* }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating thumbnail: {ex.Message}");
                return StatusCode(500, new Response(false, "An error occurred while updating the thumbnail."));
            }*/
        }




        public record UpdateThumbnailRequest(string NewThumbnail);



        [Authorize]
        [HttpGet("isFollowing/{userId}")]
        public async Task<IActionResult> IsCurrentUserFollowing(int userId)
        {
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null)
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }

            var currentUserId = int.Parse(currentUserIDClaim.Value);



            // Check if the user is trying to check if they are following themselves
            if (userId == currentUserId)
            {
                return BadRequest(new Response(false, "You cannot check if you are following yourself."));
            }

            var isFollowing = await IsUserFollowed(currentUserId, userId);

            return Ok(new { isFollowing });
        }


            [HttpGet("getFollowers")]
        public async Task<IActionResult> GetAllFollowers()
        {
            // Get the current user's ID from the claims
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null || !int.TryParse(currentUserIDClaim.Value, out var currentUserId))
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }

            // Retrieve the user IDs that the current user is following
            var followedUserIds = dbContext.Followers
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.UserId)
                .ToList();

            // Retrieve user data for the followed users
            var followedUsers = await dbContext.Auth
                .Where(u => followedUserIds.Contains(u.Id))
                .ToListAsync();

            // Do something with the 'followedUsers' data, e.g., return it in the response
            return Ok(followedUsers);
        }

        [Authorize]
        [HttpPatch("updateFirstName")]
        public async Task<IActionResult> UpdateFirstNameAsync([FromBody] UpdateFieldRequest<string> request)
        {
            return await UpdateFieldAsync(request, (user, value) => user.FirstName = value);
        }

        [Authorize]
        [HttpPatch("updateLastName")]
        public async Task<IActionResult> UpdateLastNameAsync([FromBody] UpdateFieldRequest<string> request)
        {
            return await UpdateFieldAsync(request, (user, value) => user.LastName = value);
        }

        [Authorize]
        [HttpPatch("updateEmail")]
        public async Task<IActionResult> UpdateEmailAsync([FromBody] UpdateFieldRequest<string> request)
        {
            return await UpdateFieldAsync(request, (user, value) => user.Email = value);
        }

        [Authorize]
        [HttpPatch("updatePassword")]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdateFieldRequest<string> request)
        {
            return await UpdateFieldAsync(request, (user, value) => user.Password = value);
        }

        private async Task<IActionResult> UpdateFieldAsync<T>(UpdateFieldRequest<T> request, Action<Auth, T> updateAction)
        {
            try
            {
                var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");
                if (currentUserIDClaim == null || !int.TryParse(currentUserIDClaim.Value, out var currentUserId))
                {
                    return Unauthorized(new Response(false, "You are not logged in."));
                }

                var userToUpdate = await dbContext.Auth.FindAsync(currentUserId);
                if (userToUpdate == null)
                {
                    return NotFound(new Response(false, "User not found."));
                }

                updateAction(userToUpdate, request.NewValue);

                dbContext.Update(userToUpdate);
                await dbContext.SaveChangesAsync();

                return Ok(new Response(true, $"{request.FieldName} updated successfully."));
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating {request.FieldName}: {ex.Message}");
                return StatusCode(500, new Response(false, $"An error occurred while updating {request.FieldName}."));
            }
        }

        [Authorize]
        [HttpPatch("updateProfile")]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");
                if (currentUserIDClaim == null || !int.TryParse(currentUserIDClaim.Value, out var currentUserId))
                {
                    return Unauthorized(new Response(false, "You are not logged in."));
                }

                var userToUpdate = await dbContext.Auth.FindAsync(currentUserId);
                if (userToUpdate == null)
                {
                    return NotFound(new Response(false, "User not found."));
                }

                // Update fields if they are provided in the request
                if (!string.IsNullOrEmpty(request.FirstName))
                {
                    userToUpdate.FirstName = request.FirstName;
                }

                if (!string.IsNullOrEmpty(request.LastName))
                {
                    userToUpdate.LastName = request.LastName;
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    userToUpdate.Email = request.Email;
                }

                if (!string.IsNullOrEmpty(request.Password))
                {
                    userToUpdate.Password = request.Password;
                }

                dbContext.Update(userToUpdate);
                await dbContext.SaveChangesAsync();

                return Ok(new Response(true, "Profile updated successfully."));
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating profile: {ex.Message}");
                return StatusCode(500, new Response(false, "An error occurred while updating the profile."));
            }
        }

        public record UpdateProfileRequest(string FirstName, string LastName, string Email, string Password);


        public record UpdateFieldRequest<T>(string FieldName, T NewValue);


        private async Task<bool> IsUserFollowed(int followerId, int userId)
        {
            return await dbContext.Followers.AnyAsync(f => f.FollowerId == followerId && f.UserId == userId);
        }


    }
}
