using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;
using TravelAgency.Models.ViewModels;

namespace TravelAgency.Controllers
{
    public class TripsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TripsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Trips
        public async Task<IActionResult> Index(
    string? search,
    string? category,
    string? sortOrder,
    bool showDiscountedOnly = false)
        {
            var today = DateTime.Today;
            bool isAdmin = User.IsInRole("Admin");

            var query = _context.TravelPackages
                .Include(t => t.Images)
                .Where(t => t.IsVisible) // 🔒 HIDE invisible trips
                .Where(t => t.BookingDeadline == null || t.BookingDeadline >= today)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(t => t.EndDate >= today);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t =>
                    t.Name.Contains(s) ||
                    t.Destination.Contains(s) ||
                    t.Country.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.PackageType == category);
            }

            if (showDiscountedOnly)
            {
                query = query.Where(t =>
                    t.DiscountedPrice != null &&
                    t.DiscountEndDate != null &&
                    t.DiscountEndDate >= today);
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(t => t.DiscountedPrice ?? t.BasePrice),
                "price_desc" => query.OrderByDescending(t => t.DiscountedPrice ?? t.BasePrice),
                "date_asc" => query.OrderBy(t => t.StartDate),
                "date_desc" => query.OrderByDescending(t => t.StartDate),
                "rooms_desc" => query.OrderByDescending(t => t.AvailableRooms),
                "rooms_asc" => query.OrderBy(t => t.AvailableRooms),
                "name" => query.OrderBy(t => t.Name),
                _ => query.OrderBy(t => t.StartDate)
            };

            var trips = await query.ToListAsync();

            var model = new TripsIndexViewModel
            {
                Search = search,
                Category = category,
                SortOrder = sortOrder,
                ShowDiscountedOnly = showDiscountedOnly,

                TotalTripsCount = await _context.TravelPackages.CountAsync(t => t.IsVisible),

                Categories = await _context.TravelPackages
                    .Where(t => t.IsVisible)
                    .Select(t => t.PackageType)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),

                Trips = trips.Select(t => new TripCardViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Destination = t.Destination,
                    Country = t.Country,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,

                    BasePrice = t.BasePrice,
                    DiscountedPrice =
                        t.DiscountEndDate != null && t.DiscountEndDate >= today
                            ? t.DiscountedPrice
                            : null,

                    DiscountEndDate = t.DiscountEndDate,
                    TotalRooms = t.TotalRooms,
                    AvailableRooms = t.AvailableRooms,
                    PackageType = t.PackageType,
                    AgeLimit = t.AgeLimit,
                    Description = t.Description,

                    MainImageUrl = t.Images
                        .OrderByDescending(i => i.IsMain)
                        .FirstOrDefault()?.ImageUrl
                        ?? "/images/trips/placeholder.jpg",

                    IsExpired = t.EndDate.Date < today
                }).ToList()
            };

            return View(model);
        }


        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var today = DateTime.Today;

            var trip = await _context.TravelPackages
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsVisible);

            if (trip == null)
                return NotFound();

            bool isExpired = trip.EndDate.Date < today;
            bool isBookingClosed =
                trip.BookingDeadline.HasValue &&
                trip.BookingDeadline.Value.Date < today;

            string? userId = null;
            bool alreadyBooked = false;
            bool canReview = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                userId = _userManager.GetUserId(User);

                alreadyBooked = await _context.Bookings.AnyAsync(b =>
                    b.UserId == userId &&
                    b.TravelPackageId == id &&
                    b.Status != BookingStatus.Cancelled);

                canReview = alreadyBooked &&
                    !await _context.TripReviews.AnyAsync(r =>
                        r.UserId == userId &&
                        r.TravelPackageId == id);
            }

            var reviews = await _context.TripReviews
                .Where(r => r.TravelPackageId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var vm = new TripCardViewModel
            {
                Id = trip.Id,
                Name = trip.Name,
                Destination = trip.Destination,
                Country = trip.Country,
                PackageType = trip.PackageType,
                AgeLimit = trip.AgeLimit,
                Description = trip.Description,

                BasePrice = trip.BasePrice,
                DiscountedPrice =
                    trip.DiscountEndDate != null &&
                    trip.DiscountEndDate >= today
                        ? trip.DiscountedPrice
                        : null,

                DiscountEndDate = trip.DiscountEndDate,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                AvailableRooms = trip.AvailableRooms,
                TotalRooms = trip.TotalRooms,

                MainImageUrl = trip.Images
                    .OrderByDescending(i => i.IsMain)
                    .FirstOrDefault()?.ImageUrl
                    ?? "/images/trips/placeholder.jpg",

                IsAlreadyBookedByUser = alreadyBooked,
                Reviews = reviews,
                CanReview = canReview
            };

            ViewBag.IsExpired = isExpired;
            ViewBag.IsBookingClosed = isBookingClosed;

            return View(vm);
        }


    }
}
