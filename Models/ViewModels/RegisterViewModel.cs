using System;
using System.ComponentModel.DataAnnotations;

namespace TravelAgency.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Phone]
        [RegularExpression(@"^(\+972|0)([23489]|5[0-9])\d{7}$",
            ErrorMessage = "Please enter a valid Israeli phone number")]
        public string? PhoneNumber { get; set; }


        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }


        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
