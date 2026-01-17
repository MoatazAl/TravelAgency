using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Models;

namespace TravelAgency.Data
{
    public class TravelAgencyContext : IdentityDbContext
    {
        public TravelAgencyContext(DbContextOptions<TravelAgencyContext> options)
            : base(options)
        {
        }

        public DbSet<TravelPackage> TravelPackages { get; set; }
        public DbSet<PackageImage> PackageImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<WaitingListEntry> WaitingListEntries { get; set; }
        public DbSet<TripReview> TripReviews { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

    }
}
