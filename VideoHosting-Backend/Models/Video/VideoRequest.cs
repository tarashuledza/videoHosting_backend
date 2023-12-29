namespace VideoHosting_Backend.Models.Video
{
    public class VideoRequest
    {
      
            public int AuthorId { get; set; }
             public string Title { get; set; }
            public string Description { get; set; }
            public IFormFile Thumbnail { get; set; }
            public IFormFile Video { get; set; }
        
    }
}
