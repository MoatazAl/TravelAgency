using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelAgency.Models.ViewModels
{
    public class TripCardViewModel
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
        public string MainImageUrl { get; set; } = "";

        public bool HasActiveDiscount =>
            DiscountedPrice.HasValue &&
            DiscountEndDate.HasValue &&
            DiscountEndDate.Value.Date >= DateTime.Today;

        public decimal EffectivePrice => HasActiveDiscount ? DiscountedPrice!.Value : BasePrice;
        public bool IsAlreadyBookedByUser { get; set; }
        public bool IsExpired { get; set; }
        public bool IsBookingClosed { get; set; }
        public List<TripReview> Reviews { get; set; } = new();
        public bool CanReview { get; set; }

    }

    public class TripsIndexViewModel
    {
        public List<TripCardViewModel> Trips { get; set; } = new();

        // Filters / query
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? SortOrder { get; set; }
        public bool ShowDiscountedOnly { get; set; }

        // For UI
        public int TotalTripsCount { get; set; }
        public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();
    }
}
