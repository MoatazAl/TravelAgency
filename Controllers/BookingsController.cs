using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;
using TravelAgency.Models.ViewModels;
using TravelAgency.Services;

namespace TravelAgency.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // STEP 1: CREATE (GET)
        // =========================
        public async Task<IActionResult> Create(int tripId)
        {
            var trip = await _context.TravelPackages
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return NotFound();

            if (trip.AvailableRooms <= 0)
            {
                return RedirectToAction(nameof(WaitingList), new { tripId });
            }


            var vm = new CreateBookingViewModel
            {
                TravelPackageId = trip.Id,
                PackageName = trip.Name,
                Destination = $"{trip.Destination}, {trip.Country}",
                ImageUrl = trip.Images.FirstOrDefault()?.ImageUrl is string img
                    ? (img.StartsWith("/") ? img : "/" + img)
                    : "/images/trips/placeholder.jpg",
                Price = trip.DiscountedPrice ?? trip.BasePrice,
                DepartureDate = trip.StartDate,
                ReturnDate = trip.EndDate,
                CancellationAllowedUntil = trip.StartDate.AddDays(-7)
            };

            return View(vm);
        }

        // =========================
        // STEP 2: CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirmed(CreateBookingViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // RULE: max 3 upcoming trips
            var upcomingCount = await _context.Bookings.CountAsync(b =>
                b.UserId == user.Id &&
                b.Status != BookingStatus.Cancelled &&
                b.DepartureDate > DateTime.UtcNow);

            if (upcomingCount >= 3)
            {
                TempData["Error"] = "You can only book up to 3 upcoming trips.";
                return RedirectToAction("Details", "Trips", new { id = vm.TravelPackageId });
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            var trip = await _context.TravelPackages
                .FirstOrDefaultAsync(t => t.Id == vm.TravelPackageId);

            if (trip == null)
            {
                await tx.RollbackAsync();
                return NotFound();
            }

            // RULE: prevent duplicate active booking of same trip
            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.UserId == user.Id &&
                b.TravelPackageId == trip.Id &&
                b.Status != BookingStatus.Cancelled);

            if (alreadyBooked)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "You already have an active booking for this trip.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }

            // RULE: last-room protection
            if (trip.AvailableRooms <= 0)
            {
                await tx.RollbackAsync();
                return RedirectToAction("Join", "WaitingList", new { tripId = trip.Id });
            }

            var booking = new Booking
            {
                UserId = user.Id,
                TravelPackageId = trip.Id,
                TotalPrice = vm.Price,
                DepartureDate = trip.StartDate,
                CancellationAllowedUntil = vm.CancellationAllowedUntil,
                Status = BookingStatus.PendingPayment
            };

            trip.AvailableRooms -= 1;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Confirmation), new { id = booking.Id });
        }

        // =========================
        // STEP 3: CONFIRMATION
        // =========================
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // =========================
        // MY BOOKINGS (Dashboard)
        // =========================
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var bookings = await _context.Bookings
                .Include(b => b.TravelPackage)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // =========================
        // BOOKING DETAILS
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // =========================
        // CANCEL BOOKING
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
                return NotFound();

            bool canCancel =
                booking.Status == BookingStatus.PendingPayment ||
                (booking.Status == BookingStatus.Confirmed &&
                 DateTime.Today <= booking.CancellationAllowedUntil.Date);

            if (!canCancel)
            {
                TempData["Error"] = "This booking can no longer be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = BookingStatus.Cancelled;
            booking.TravelPackage.AvailableRooms += 1;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(MyBookings));
        }


        [Authorize]
        public async Task<IActionResult> DownloadItinerary(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.UserId == user.Id &&
                    b.Status == BookingStatus.Confirmed);

            if (booking == null)
                return NotFound();

            var pdf = ItineraryPdfService.Generate(booking);

            return File(pdf, "application/pdf", $"Itinerary_{booking.Id}.pdf");
        }

        [Authorize]
        public async Task<IActionResult> WaitingList(int tripId)
        {
            var trip = await _context.TravelPackages
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return NotFound();

            int waitingCount = await _context.WaitingListEntries
                .CountAsync(w => w.TravelPackageId == tripId);

            ViewBag.WaitingCount = waitingCount;

            return View(trip);
        }
    }
}
