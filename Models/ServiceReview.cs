using System;

namespace TravelAgency.Models
{
    public class ServiceReview
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
