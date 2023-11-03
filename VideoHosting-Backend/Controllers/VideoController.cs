using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using VideoHosting_Backend.Models.Video;


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

        [HttpGet("{id}")]
        public async Task<IActionResult> getVideoById(int id)
        {
            var video = await _context.Set<Video>().FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            return Ok(video);
        }
    }
}
