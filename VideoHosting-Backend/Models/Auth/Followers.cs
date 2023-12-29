namespace VideoHosting_Backend.Models.Auth
{
    public class Followers
    {
        public int Id { get; set; }

        // FollowerId is the user who is following
        public int FollowerId { get; set; }

        // UserId is the user who is being followed
        public int UserId { get; set; }

        public virtual Auth Follower { get; set; }
        public virtual Auth User { get; set; }
    }
}
