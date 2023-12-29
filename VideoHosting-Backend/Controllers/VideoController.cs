using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlX.XDevAPI.Common;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using VideoHosting_Backend.Models.Auth;
using VideoHosting_Backend.Models.Video;
using VideoHosting_Backend.Services;
using static Google.Apis.Drive.v3.Data.File.ContentHintsData;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static VideoHosting_Backend.Controllers.AuthController;

namespace VideoHosting_Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class VideoController : Controller
    {
        private readonly StoreDbContext _context;
        public VideoController(StoreDbContext dbContext)
        {
            this._context = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddVideo(
            [FromForm] VideoRequest videoRequest
          )
        {
            if (videoRequest == null)
            {
                return BadRequest("Invalid request. Please provide both a file and video data.");
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    videoRequest.Video.CopyTo(ms);
                    var videoByte = ms.ToArray();



                    var videoData = new Video
                    {
                        AuthorId = videoRequest.AuthorId,
                        VideoData = videoByte,
                        PublicationDate = DateTime.Now,
                        Description = videoRequest.Description,
                        Thumbnail = ByteConverterService.ConvertFileToByteAsync(videoRequest.Thumbnail),
                        Title = videoRequest.Title,
                    };

                    _context.Video.Add(videoData);
                    await _context.SaveChangesAsync();
                }
                return Ok("Files uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVideo([FromBody] VideoReq video)
        {
            if (video == null || video.VideoId <= 0)
            {
                return BadRequest("Invalid video data");
            }

            try
            {
                // Здійснюємо пошук відео за ідентифікатором
                var existingVideo = await _context.Video.FindAsync(video.VideoId);

                if (existingVideo == null)
                {
                    return NotFound($"Video with ID {video.VideoId} not found");
                }

                // Оновлюємо властивості існуючого відео
                existingVideo.Title = video.Title;
                existingVideo.Description = video.Description;

                // Зберігаємо зміни у базі даних
                await _context.SaveChangesAsync();

                return Ok("Video updated successfully");
            }
            catch (Exception ex)
            {
                // Обробка помилок, наприклад, логування
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        [HttpGet("followingVideos")]
        public async Task<IActionResult> GetFollowedUsers()
        {
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null || !int.TryParse(currentUserIDClaim.Value, out var currentUserId))
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }

            var followedUserIds = _context.Followers
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.UserId)
                .ToList();


            var followingVideos = _context.Video
                .Where(v => followedUserIds.Contains(v.AuthorId))
                .ToList();

            var combinedData = followingVideos.Select(v =>
            {
                var author = _context.Auth.FirstOrDefault(a => a.Id == v.AuthorId);


                return new VideoAndAuthorData
                {
                    VideoId = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    //VideoData = v.VideoData,
                    Thumbnail = v.Thumbnail,
                    PublicationDate = DateServices.getDifferenceBetweenDates(v.PublicationDate),

                    AuthorId = v.AuthorId,
                    FirstName = author?.FirstName,
                    LastName = author?.LastName,
                    AuthorThumbnail = author?.Thumbnail
                };



            }).ToList();


            return Ok(combinedData);


        }


        [HttpGet("{id}")]
        public async Task<IActionResult> getVideoById(int id)
        {
            try
            {
                var video = await _context.Video.FindAsync(id);
                if (video == null)
                {
                    return NotFound();
                }





                var userClaimsList = HttpContext.User.Claims.Select(x => new UserClaim(x.Type, x.Value)).ToList();

                var emailClaim = userClaimsList.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                var firstNameClaim = userClaimsList.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                var lastNameClaim = userClaimsList.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                var userIDClaim = userClaimsList.FirstOrDefault(c => c.Type == "Id");

                if (userIDClaim == null)
                {
                    return Unauthorized("You are not authorized to view this user.");
                }

                var email = emailClaim?.Value;
                var firstName = firstNameClaim?.Value;
                var lastName = lastNameClaim?.Value;

                int currentUserId = int.Parse(userIDClaim.Value);
                bool isFollowing = _context.Followers.Any(f => f.FollowerId == currentUserId && f.UserId == video.AuthorId);
                var author = await _context.Auth.FindAsync(video.AuthorId);

                var likes = _context.Likes.Where(l => l.VideoId == id).ToList();
                var likesCount = likes.Count();

                var dislikes = _context.Dislikes.Where(d => d.VideoId == id).ToList();
                var dislikesCount = dislikes.Count();
                var isLiked = _context.Likes.Where(l => l.VideoId == id && l.UserId == int.Parse(userIDClaim.Value)).FirstOrDefaultAsync();
                var isDisliked = _context.Dislikes.Where(l => l.VideoId == id && l.UserId == int.Parse(userIDClaim.Value)).FirstOrDefaultAsync();

                var videoData = new VideoWithLikes
                {
                    Id = video.Id,
                    Title = video.Title,
                    Description = video.Description,
                    VideoData = video.VideoData,
                    Thumbnail = video.Thumbnail,
                    PublicationDate = DateServices.getDifferenceBetweenDates(video.PublicationDate),
                    LikesCount = likesCount,
                    DislikesCount = dislikesCount,
                    AuthorId = video.AuthorId,
                };


                return Ok(new
                {
                    video = videoData,
                    author = new { author.FirstName, author.LastName, author.Thumbnail, author.Subscribers, isFollowing },
                });

            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getAllVideos/{userId}")]
        public async Task<IActionResult> GetAllVideosByUserId(int userId)
        {
            var videos = _context.Video.Where(v => v.AuthorId == userId).ToList();

            string date = "";


            var combinedData = videos.Select(v =>
            {

                var likes = _context.Likes.Where(l => l.VideoId == v.Id).ToList();
                var likesCount = likes.Count();

                var dislikes = _context.Dislikes.Where(d => d.VideoId == v.Id).ToList();
                var dislikesCount = dislikes.Count();
                using (var stream = new MemoryStream(v.Thumbnail))
                {
                    var image = System.Drawing.Image.FromStream(stream);
                    var thumbnailData = ByteConverterService.ImageToByteArray(image);

                    var author = _context.Auth.FirstOrDefault(a => a.Id == v.AuthorId);
                    date = DateServices.getDifferenceBetweenDates(v.PublicationDate);
                    return new VideoAndAuthorData
                    {
                        VideoId = v.Id,
                        Title = v.Title,
                        Description = v.Description,
                        // VideoData = v.VideoData,
                        Thumbnail = thumbnailData,
                        PublicationDate = date,
                        AuthorId = v.AuthorId,
                        FirstName = author?.FirstName,
                        LastName = author?.LastName,
                        AuthorThumbnail = author?.Thumbnail,
                        LikesCount = likesCount,
                        DislikesCount = dislikesCount,
                    };
                }
            }).ToList();

            return Ok(combinedData);
        }
        [HttpPatch]
        [Route("addLike")]
        public async Task<IActionResult> AddLike(int videoId)
        {
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null)
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }
            var userId = int.Parse(currentUserIDClaim.Value);

            var likeByUser = await _context.Likes.Where(l => l.VideoId == videoId && l.UserId == userId).FirstOrDefaultAsync();
            var dislikeByUser = await _context.Dislikes.Where(l => l.VideoId == videoId && l.UserId == userId).FirstOrDefaultAsync();

            if (likeByUser != null)
            {
                _context.Likes.Remove(likeByUser);
                _context.SaveChanges();

                return Ok(new { isSuccess = true, message = "Unlike" });
            }
            else
            {
                if (dislikeByUser != null)
                {
                    _context.Dislikes.Remove(dislikeByUser);
                }
                var likes = new Likes
                {
                    VideoId = videoId,
                    UserId = userId,
                };

                _context.Likes.Add(likes);
                _context.SaveChanges();

                return Ok(new { isSuccess = true, message = "Like" });

            }
        }

        [HttpPatch]
        [Route("addDislike")]

        public async Task<IActionResult> AddDislike(int videoId)
        {
            var currentUserIDClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (currentUserIDClaim == null)
            {
                return Unauthorized(new Response(false, "You are not logged in."));
            }
            var userId = int.Parse(currentUserIDClaim.Value);

            var likeByUser = await _context.Likes.Where(l => l.VideoId == videoId && l.UserId == userId).FirstOrDefaultAsync();
            var dislikeByUser = await _context.Dislikes.Where(l => l.VideoId == videoId && l.UserId == userId).FirstOrDefaultAsync();

            if (dislikeByUser != null)
            {
                _context.Dislikes.Remove(dislikeByUser);
                _context.SaveChanges();

                return Ok(new { isSuccess = true, message = "Unlike" });
            }
            else
            {
                if (likeByUser != null)
                {
                    _context.Likes.Remove(likeByUser);
                }
                var dislike = new Dislikes
                {
                    VideoId = videoId,
                    UserId = userId,
                };

                _context.Dislikes.Add(dislike);
                _context.SaveChanges();

                return Ok(new { isSuccess = true, message = "Like" });

            }
        }
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchProducts(string? query, string type)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Ok(new List<VideoAndAuthorData>()); // Return an empty list if the query is empty or null
            }

            List<System.Object> datas = new List<System.Object>();

            if (type.ToLower() == "video")
            {
                var queryable = _context.Video
                    .Where(v => EF.Functions.Like(v.Title, $"%{query}%") ||
                                EF.Functions.Like(v.Description, $"%{query}%"));

                var videos = await queryable.ToListAsync();

                if (videos.Count == 0)
                {
                    return NotFound("No products found matching the search query and price range.");
                }

                foreach (var v in videos)
                {
                    var author = await _context.Auth.FindAsync(v.AuthorId);

                    var data = new VideoAndAuthorData
                    {
                        Title = v.Title,
                        Description = v.Description,
                        AuthorId = author.Id,
                        //VideoData = v.VideoData,
                        Thumbnail = v.Thumbnail,
                        PublicationDate = DateServices.getDifferenceBetweenDates(v.PublicationDate),
                        VideoId = v.Id,
                        FirstName = author.FirstName,
                        LastName = author.LastName,
                        AuthorThumbnail = author.Thumbnail,
                        Subscribers = author.Subscribers
                    };

                    datas.Add(data);
                }
            }
            else if (type.ToLower() == "channel")
            {
                var queryable = _context.Auth
                    .Where(a => EF.Functions.Like(a.FirstName + " " + a.LastName, $"%{query}%"));

                var channels = await queryable.ToListAsync();
                datas.AddRange(channels);
            }
            else
            {
                return BadRequest("Invalid type parameter. Use 'video' or 'channel'.");
            }

            datas = datas.OrderByDescending(data => GetSimilarityScore(data, query)).ToList();
            return Ok(datas);
        }

        private double GetSimilarityScore(System.Object data, string query)
        {
            // Extracting properties from the generic type
            string title = (data.GetType().GetProperty("Title")?.GetValue(data) as string) ?? "";
            string description = (data.GetType().GetProperty("Description")?.GetValue(data) as string) ?? "";

            // Transforming strings into term dictionaries (use tokenization and other methods as needed)
            var queryTerms = GetTerms(query);
            var titleTerms = GetTerms(title);
            var descriptionTerms = GetTerms(description);

            // Combining all terms for comparison
            var allTerms = queryTerms.Union(titleTerms).Union(descriptionTerms).Distinct().ToList();

            // Creating vectors for comparison
            var queryVector = CreateVector(queryTerms, allTerms);
            var titleVector = CreateVector(titleTerms, allTerms);
            var descriptionVector = CreateVector(descriptionTerms, allTerms);

            // Calculating cosine similarity between the query and text fields
            double titleSimilarity = CosineSimilarity(queryVector, titleVector);
            double descriptionSimilarity = CosineSimilarity(queryVector, descriptionVector);

            // Returning the average similarity value
            return (titleSimilarity + descriptionSimilarity) / 2.0;
        }


        private List<string> GetTerms(string text)
        {
            // Простий метод для токенізації рядка (розділення на слова)
            return text.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(term => term.ToLower())
                       .ToList();
        }

        private Dictionary<string, int> CreateVector(List<string> terms, List<string> allTerms)
        {
            // Створення вектора термінів для подальшого порівняння
            var vector = new Dictionary<string, int>();
            foreach (var term in allTerms)
            {
                vector[term] = terms.Count(t => t == term);
            }
            return vector;
        }

        private double CosineSimilarity(Dictionary<string, int> vectorA, Dictionary<string, int> vectorB)
        {
            // Розрахунок косинусної схожості між двома векторами
            double dotProduct = vectorA.Sum(kv => kv.Value * vectorB.GetValueOrDefault(kv.Key, 0));
            double magnitudeA = Math.Sqrt(vectorA.Values.Sum(v => v * v));
            double magnitudeB = Math.Sqrt(vectorB.Values.Sum(v => v * v));

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0.0; // Якщо один з векторів має нульову довжину, схожість - нуль
            }

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }

}


