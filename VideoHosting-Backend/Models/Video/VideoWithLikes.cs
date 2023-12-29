using Microsoft.AspNetCore.Mvc;

namespace VideoHosting_Backend.Models.Video
{
    public class VideoWithLikes
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public byte[] VideoData { get; set; }
        public byte[] Thumbnail { get; set; }
        public string PublicationDate { get; set; }
        public int DislikesCount { get; set; }
        public int LikesCount { get; set; }
        public int AuthorId { get; set; }
    }
}
