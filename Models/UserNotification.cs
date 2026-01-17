namespace TravelAgency.Models
{
    public class UserNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
