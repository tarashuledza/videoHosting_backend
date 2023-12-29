
using Microsoft.EntityFrameworkCore;
using VideoHosting_Backend.Models.Auth;
using VideoHosting_Backend.Models.Review;
using VideoHosting_Backend.Models.Video;
using Web_API.Models.ProfileModels;

public class StoreDbContext : DbContext
{

    public DbSet<Auth> Auth { get; set; }
    public DbSet<Likes> Likes { get; set; }
    public DbSet<Dislikes> Dislikes { get; set; }
    public DbSet<Video> Video { get; set; }
    public DbSet<Review> Review { get; set; }
    public DbSet<Followers> Followers { get; set; }
   

    // Add your connection string in the constructor
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships between entities if needed
    }
}
