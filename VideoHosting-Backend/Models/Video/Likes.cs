namespace VideoHosting_Backend.Models.Video
{
    public class Likes
    {
        public int? Id { get; set; }
        public int VideoId { get; set; }
        public int UserId { get; set; }

    }
}
