using Microsoft.AspNetCore.Identity;
using System;

namespace TravelAgency.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
    }
}
