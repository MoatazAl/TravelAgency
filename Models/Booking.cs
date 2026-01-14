using System;

namespace TravelAgency.Models
{
    public enum BookingStatus { PendingPayment, Confirmed, Cancelled }

    public class Booking
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public decimal TotalPrice { get; set; }

        public int TravelPackageId { get; set; }
        public TravelPackage TravelPackage { get; set; } = null!;

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

        public DateTime DepartureDate { get; set; }
        public DateTime CancellationAllowedUntil { get; set; }

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
    }
}
