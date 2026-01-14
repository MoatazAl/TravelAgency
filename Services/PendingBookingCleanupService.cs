using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;

namespace TravelAgency.Services
{
    public class PendingBookingCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PendingBookingCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TravelAgencyContext>();

                var expired = await context.Bookings
                    .Include(b => b.TravelPackage)
                    .Where(b =>
                        b.Status == BookingStatus.PendingPayment &&
                        b.BookingDate < DateTime.UtcNow.AddMinutes(-15))
                    .ToListAsync();

                foreach (var booking in expired)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.TravelPackage.AvailableRooms += 1;
                }

                await context.SaveChangesAsync();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
