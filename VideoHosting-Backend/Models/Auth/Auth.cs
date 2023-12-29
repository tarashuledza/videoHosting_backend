using System.Reflection.Metadata;

namespace VideoHosting_Backend.Models.Auth
{
    public class Auth
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Thumbnail { get; set; }
        public int Subscribers { get; set; }
    }
}

