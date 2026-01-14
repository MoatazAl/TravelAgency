using System;
using System.Collections.Generic;

namespace TravelAgency.Models
{
    public enum PaymentStatus { Success, Failed }

    public class Payment
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string TransactionReference { get; set; } = null!;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
