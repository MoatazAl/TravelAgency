using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelAgency.Data;
using TravelAgency.Models;
using TravelAgency.Models.ViewModels;
using TravelAgency.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace TravelAgency.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BookingsController> _logger;

        // Optional: if you already have a real email service, inject it.
        private readonly IEmailSender _emailSender;

        public BookingsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager,
            ILogger<BookingsController> logger,
            IEmailSender emailSender
            )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        // =========================
        // STEP 1: CREATE (GET)
        // =========================
        public async Task<IActionResult> Create(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var trip = await _context.TravelPackages
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return NotFound();

            var today = DateTime.UtcNow.Date;

            // 🚫 BLOCK: booking deadline passed
            if (trip.BookingDeadline.HasValue &&
                trip.BookingDeadline.Value.Date < today)
            {
                TempData["Error"] = "Booking period for this trip has ended.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }


            // 🚫 BLOCK: already booked this trip
            bool alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.UserId == user.Id &&
                b.TravelPackageId == trip.Id &&
                b.Status != BookingStatus.Cancelled);

            if (alreadyBooked)
            {
                TempData["Error"] = "You already have an active booking for this trip.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }

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


        // helper: what counts as "active booking" for constraints
        private static bool IsActiveStatus(BookingStatus s) =>
            s != BookingStatus.Cancelled;

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

            var trip = await _context.TravelPackages
                .FirstOrDefaultAsync(t => t.Id == vm.TravelPackageId);

            if (trip == null)
                return NotFound();

            var today = DateTime.UtcNow.Date;

            // 🚫 BLOCK: trip already ended
            if (trip.EndDate.Date < today)
            {
                TempData["Error"] = "This trip has already ended.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }

            // RULE 0: booking deadline
            if (trip.BookingDeadline.HasValue && DateTime.Today > trip.BookingDeadline.Value)
            {
                TempData["Error"] = "Booking period for this trip has ended.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }

            // RULE 1: max 3 upcoming trips
            var upcomingCount = await _context.Bookings.CountAsync(b =>
                b.UserId == user.Id &&
                b.Status != BookingStatus.Cancelled &&
                b.DepartureDate > DateTime.UtcNow);

            if (upcomingCount >= 3)
            {
                TempData["Error"] = "You can only book up to 3 upcoming trips.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }

            // RULE 2: prevent duplicate booking
            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.UserId == user.Id &&
                b.TravelPackageId == trip.Id &&
                b.Status != BookingStatus.Cancelled);

            if (alreadyBooked)
            {
                TempData["Error"] = "You already have an active booking for this trip.";
                return RedirectToAction("Details", "Trips", new { id = trip.Id });
            }

            // RULE 3: age restriction
            if (trip.AgeLimit.HasValue)
            {
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (profile == null)
                {
                    TempData["Error"] = "User profile not found.";
                    return RedirectToAction("Details", "Trips", new { id = trip.Id });
                }

                int age = DateTime.Today.Year - profile.DateOfBirth.Year;
                if (profile.DateOfBirth.Date > DateTime.Today.AddYears(-age))
                    age--;

                if (age < trip.AgeLimit.Value)
                {
                    TempData["Error"] = $"This trip requires minimum age of {trip.AgeLimit}.";
                    return RedirectToAction("Details", "Trips", new { id = trip.Id });
                }
            }

            // TRANSACTION: last-room protection
            using var tx = await _context.Database.BeginTransactionAsync();

            trip = await _context.TravelPackages
                .FirstOrDefaultAsync(t => t.Id == vm.TravelPackageId);

            if (trip == null)
            {
                await tx.RollbackAsync();
                return NotFound();
            }

            if (trip.AvailableRooms <= 0)
            {
                await tx.RollbackAsync();
                return RedirectToAction("Join", "WaitingList", new { tripId = trip.Id });
            }

            // PRICE: respect discount expiration
            decimal finalPrice =
                (trip.DiscountedPrice.HasValue &&
                 trip.DiscountEndDate.HasValue &&
                 trip.DiscountEndDate >= DateTime.Today)
                ? trip.DiscountedPrice.Value
                : trip.BasePrice;

            var booking = new Booking
            {
                UserId = user.Id,
                TravelPackageId = trip.Id,
                TotalPrice = finalPrice,
                DepartureDate = trip.StartDate,
                CancellationAllowedUntil = trip.StartDate.AddDays(-7),
                BookingDate = DateTime.UtcNow,
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
        // MY BOOKINGS (Dashboard) + WAITING LIST POSITION
        // =========================
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // =========================
            // LOAD BOOKINGS FIRST
            // =========================
            var bookings = await _context.Bookings
                .Include(b => b.TravelPackage)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // =========================
            // CHECK IF ACTION IS NEEDED
            // =========================
            bool hasPendingPayment = bookings.Any(b =>
                b.Status == BookingStatus.PendingPayment);

            // =========================
            // LOAD USER NOTIFICATIONS
            // =========================
            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();

            // =========================
            // NOTIFICATION RULE
            // =========================
            if (!hasPendingPayment)
            {
                // Auto-cleanup: mark notifications as read
                foreach (var n in notifications)
                    n.IsRead = true;

                if (notifications.Any())
                    await _context.SaveChangesAsync();

                ViewBag.Notifications = new List<UserNotification>();
            }
            else
            {
                ViewBag.Notifications = notifications;
            }

            // =========================
            // LOAD WAITING LIST INFO
            // =========================
            var userWaiting = await _context.WaitingListEntries
                .Include(w => w.TravelPackage)
                .Where(w => w.UserId == user.Id)
                .ToListAsync();

            var waitingAll = await _context.WaitingListEntries
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

            // =========================
            // BUILD VIEW MODEL
            // =========================
            var items = new List<MyBookingsItemViewModel>();

            foreach (var b in bookings)
            {
                items.Add(new MyBookingsItemViewModel
                {
                    Booking = b
                });
            }

            foreach (var w in userWaiting)
            {
                var sameTrip = waitingAll
                    .Where(x => x.TravelPackageId == w.TravelPackageId)
                    .ToList();

                var pos = sameTrip.FindIndex(x => x.Id == w.Id) + 1;

                items.Add(new MyBookingsItemViewModel
                {
                    Waiting = w,
                    WaitingCount = sameTrip.Count,
                    WaitingPosition = pos
                });
            }

            return View(items);
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
        .ThenInclude(t => t.Images)
    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);
            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // =========================
        // CANCEL BOOKING + AUTO-ASSIGN NEXT WAITLIST USER
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            using var tx = await _context.Database.BeginTransactionAsync();

            var booking = await _context.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
            {
                await tx.RollbackAsync();
                return NotFound();
            }

            bool canCancel =
                booking.Status == BookingStatus.PendingPayment ||
                (booking.Status == BookingStatus.Confirmed &&
                 DateTime.Today <= booking.CancellationAllowedUntil.Date);

            if (!canCancel)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "This booking can no longer be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 1️⃣ Cancel booking
            booking.Status = BookingStatus.Cancelled;
            booking.TravelPackage.AvailableRooms += 1;

            await _context.SaveChangesAsync();

            // 2️⃣ Promote first waiting list user (if exists)
            var nextInLine = await _context.WaitingListEntries
                .Where(w => w.TravelPackageId == booking.TravelPackageId)
                .OrderBy(w => w.CreatedAt)
                .FirstOrDefaultAsync();

            if (nextInLine != null)
            {
                var promotedBooking = new Booking
                {
                    UserId = nextInLine.UserId,
                    TravelPackageId = booking.TravelPackageId,
                    TotalPrice = booking.TravelPackage.DiscountedPrice
                                 ?? booking.TravelPackage.BasePrice,
                    DepartureDate = booking.TravelPackage.StartDate,
                    CancellationAllowedUntil = booking.TravelPackage.StartDate.AddDays(-7),
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.PendingPayment
                };

                booking.TravelPackage.AvailableRooms -= 1;

                _context.Bookings.Add(promotedBooking);
                _context.WaitingListEntries.Remove(nextInLine);

                // 🔔 In-app notification
                _context.UserNotifications.Add(new UserNotification
                {
                    UserId = nextInLine.UserId,
                    Message = "Good news! A room is now available for a trip you were waiting for. Please complete payment within the allowed time.",
                    IsRead = false
                });

                await _context.SaveChangesAsync();

                // 📧 EMAIL NOTIFICATION (ConsoleEmailSender)
                var promotedUser = await _userManager.FindByIdAsync(nextInLine.UserId);

                if (promotedUser?.Email != null)
                {
                    await _emailSender.SendEmailAsync(
                        promotedUser.Email,
                        "A room is now available!",
                        $"Good news!\n\nA room is now available for the trip \"{booking.TravelPackage.Name}\".\n" +
                        $"Please log in and complete payment within the allowed time.\n\n— NoorAgency"
                    );
                }

                _logger.LogInformation(
                    "WAITLIST PROMOTION: User {UserId} promoted to booking {BookingId} for trip {TripId}",
                    nextInLine.UserId,
                    promotedBooking.Id,
                    booking.TravelPackageId
                );
            }

            await tx.CommitAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(MyBookings));
        }



        // =========================
        // PDF ITINERARY
        // =========================
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

        // =========================
        // WAITING LIST PAGE (count)
        // =========================
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
