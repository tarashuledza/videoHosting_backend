namespace VideoHosting_Backend.Models.Video
{
    public class VideoAndAuthorData
    {
        
            public int VideoId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            //public byte[] VideoData { get; set; }
            public byte[] Thumbnail { get; set; }
            public string PublicationDate { get; set; }
            public int AuthorId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AuthorThumbnail { get; set; }
            public int Subscribers { get; set; }
        public int? LikesCount { get; set; }
        public int? DislikesCount { get; set;}
    }
}
