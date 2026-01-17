using System;

namespace TravelAgency.Models.ViewModels
{
    public class CreateBookingViewModel
    {
        public int TravelPackageId { get; set; }

        public string PackageName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = "/images/trips/placeholder.jpg";

        public decimal Price { get; set; }

        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int? AgeLimit { get; set; }
        public DateTime? BookingDeadline { get; set; }

        public DateTime CancellationAllowedUntil { get; set; }
    }
}
