using VideoHosting_Backend.Models.Auth;

namespace VideoHosting_Backend.Models.Review
{
    public class Review
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }

    }
}
