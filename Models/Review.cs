using Microsoft.AspNetCore.Identity;

namespace AgriChoice.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
