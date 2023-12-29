namespace VideoHosting_Backend.Models.Review
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public string AuthorFirstName { get; set; }
        public string AuthorLastName { get; set; }
        public byte[] AuthorThumbnail { get; set; }

        //public string authorThumbnail { get; set; }



    }
}
