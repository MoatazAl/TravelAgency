using System;
using System.Collections.Generic;

namespace TravelAgency.Models
{
    public class TravelPackage
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public string Country { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal BasePrice { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }

        public string PackageType { get; set; } = null!;
        public int? AgeLimit { get; set; }
        public string Description { get; set; } = null!;

        public bool IsVisible { get; set; } = true;
        public int PopularityScore { get; set; } = 0;

        public ICollection<PackageImage> Images { get; set; } = new List<PackageImage>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<WaitingListEntry> WaitingList { get; set; } = new List<WaitingListEntry>();
    }
}
