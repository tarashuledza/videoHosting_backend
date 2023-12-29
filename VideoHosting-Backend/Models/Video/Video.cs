using System.Reflection.Metadata;

namespace VideoHosting_Backend.Models.Video
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public byte[] VideoData { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime PublicationDate { get; set; }
        public int AuthorId { get; set; }
    }
}
