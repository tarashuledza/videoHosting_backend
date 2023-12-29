using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoHosting_Backend.Models.Review;
using VideoHosting_Backend.Models.Video;
using VideoHosting_Backend.Services;
using static VideoHosting_Backend.Controllers.AuthController;

namespace VideoHosting_Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ReviewController : Controller
    {
        private readonly StoreDbContext _context;
        public ReviewController(StoreDbContext dbContext )
        {
            _context = dbContext;
        }
       /* [HttpGet("{videoId}")]
        public async Task<IActionResult> GetReviewByVideoId(int videoId)
        {
            var reviews = await _context.Review
                .Where(item => item.VideoId == videoId)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
            {
                return NotFound();
            }

            var dataList = new List<ReviewResponse>(); // List to store the reviews

            foreach (var review in reviews)
            {
                var author = await _context.Auth.FindAsync(review.AuthorId);

                if (author != null)
                {
                    var data = new ReviewResponse
                    {
                        AuthorId = review.AuthorId,
                        Id = review.Id,
                        VideoId = review.VideoId,
                        Description = review.Description,
                        AuthorLastName = author.LastName,
                        AuthorFirstName = author.FirstName,
                        AuthorThumbnail = author.Thumbnail
                    };

                    dataList.Add(data); // Add the review to the list
                }
            }

            return Ok(dataList.ToArray()); // Return the array of reviews
        }*/

       /* [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequest review)
        {
            if (review == null)
            {
                return BadRequest("Invalid review data");
            }

            var userClaimsList = HttpContext.User.Claims.Select(x => new UserClaim(x.Type, x.Value)).ToList();

            var userIDClaim = userClaimsList.FirstOrDefault(c => c.Type == "Id");
            var userId = userIDClaim.Value;

            var video = await _context.Video.FindAsync(review.VideoId);

            if (video == null)
            {
                return BadRequest("Video doesn't exist!");
            }
            _context.Review.Add(new Review { AuthorId = int.Parse(userId), Description = review.Description, VideoId = review.VideoId });

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviewByVideoId), new { videoId = review.VideoId }, review);
        }
*/
    }
}
