using System;

namespace TravelAgency.Models
{
    public class WaitingListEntry
    {
        public int Id { get; set; }

        public int TravelPackageId { get; set; }
        public TravelPackage TravelPackage { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsNotified { get; set; } = false;
    }
}
