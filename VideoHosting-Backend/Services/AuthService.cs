using static VideoHosting_Backend.Controllers.AuthController;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using VideoHosting_Backend.Models.Auth;

namespace VideoHosting_Backend.Services
{
    public class AuthService 
    {
        private readonly StoreDbContext _dbContext;

        public AuthService(StoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Authorize]
        public async Task<Auth> GetUserById(int userId)
        {

            var user = await _dbContext.Auth.FindAsync(userId);
            if (user == null)
            {
                return null; // Return null instead of creating a NotFound response here
            }

           

            return user;
        }
    }
}
