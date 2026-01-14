using System;

namespace TravelAgency.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public int TravelPackageId { get; set; }

        public TravelPackage TravelPackage { get; set; } = null!;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
