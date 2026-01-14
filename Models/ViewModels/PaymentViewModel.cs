using System;
using System.ComponentModel.DataAnnotations;

namespace TravelAgency.Models.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Required]
        [Range(2026, 2050)]
        public int ExpiryYear { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must be 3 digits.")]
        public string CVV { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = "CreditCard";

        public decimal Amount { get; set; }
        public string TripName { get; set; } = string.Empty;
    }
}
