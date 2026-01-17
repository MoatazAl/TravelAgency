namespace TravelAgency.Models.ViewModels
{
    public class AdminUserViewModel
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;

        public bool IsLocked { get; set; }

        public int BookingCount { get; set; }
    }
}
