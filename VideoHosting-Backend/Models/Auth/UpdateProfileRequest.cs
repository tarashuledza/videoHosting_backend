using System.Reflection.Metadata;

namespace Web_API.Models.ProfileModels
{
    public class UpdateProfileRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }


    }
}
