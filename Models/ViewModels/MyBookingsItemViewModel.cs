using TravelAgency.Models;

namespace TravelAgency.Models.ViewModels
{
    public class MyBookingsItemViewModel
    {
        // If Booking != null => real booking row
        public Booking? Booking { get; set; }

        // If Waiting != null => waiting list row
        public WaitingListEntry? Waiting { get; set; }

        public int? WaitingPosition { get; set; } // 1-based
        public int? WaitingCount { get; set; }    // total people waiting for that trip
    }
}
