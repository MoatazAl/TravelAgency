using System;

namespace TravelAgency.Models.ViewModels
{
    public class WaitingListAdminVM
    {
        public int EntryId { get; set; }

        public string TripTitle { get; set; } = "";
        public int AvailableRooms { get; set; }

        public string UserEmail { get; set; } = "";
        public DateTime RequestedAt { get; set; }

        public int Position { get; set; }
    }
}
