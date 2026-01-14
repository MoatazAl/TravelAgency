using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models.ViewModels;

namespace TravelAgency.Controllers
{
    public class TripsController : Controller
    {
        private readonly TravelAgencyContext _context;

        public TripsController(TravelAgencyContext context)
        {
            _context = context;
        }

        // GET: /Trips
        public async Task<IActionResult> Index(
            string? search,
            string? category,
            string? sortOrder,
            bool showDiscountedOnly = false)
        {
            // -------------------------------
            // BASE QUERY (SQL SAFE)
            // -------------------------------
            var query = _context.TravelPackages
                .Include(t => t.Images)
                .AsQueryable();

            // --- Search ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t =>
                    t.Name.Contains(s) ||
                    t.Destination.Contains(s) ||
                    t.Country.Contains(s));
            }

            // --- Category ---
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.PackageType == category);
            }

            // --- Discounted only ---
            if (showDiscountedOnly)
            {
                var today = DateTime.Today;
                query = query.Where(t =>
                    t.DiscountedPrice != null &&
                    t.DiscountEndDate != null &&
                    t.DiscountEndDate.Value.Date >= today);
            }

            // -------------------------------
            // 🚨 SWITCH TO MEMORY (IMPORTANT)
            // -------------------------------
            var tripsQuery = query.AsEnumerable();

            // -------------------------------
            // SORTING (SAFE IN C#)
            // -------------------------------
            tripsQuery = sortOrder switch
            {
                "price_asc"  => tripsQuery.OrderBy(t => t.DiscountedPrice ?? t.BasePrice),
                "price_desc" => tripsQuery.OrderByDescending(t => t.DiscountedPrice ?? t.BasePrice),
                "date_asc"   => tripsQuery.OrderBy(t => t.StartDate),
                "date_desc"  => tripsQuery.OrderByDescending(t => t.StartDate),
                "rooms_desc" => tripsQuery.OrderByDescending(t => t.AvailableRooms),
                "rooms_asc"  => tripsQuery.OrderBy(t => t.AvailableRooms),
                "name"       => tripsQuery.OrderBy(t => t.Name),
                _            => tripsQuery.OrderBy(t => t.StartDate)
            };

            var trips = tripsQuery.ToList();

            // -------------------------------
            // VIEW MODEL
            // -------------------------------
            var model = new TripsIndexViewModel
            {
                Search = search,
                Category = category,
                SortOrder = sortOrder,
                ShowDiscountedOnly = showDiscountedOnly,
                TotalTripsCount = await _context.TravelPackages.CountAsync(),
                Categories = await _context.TravelPackages
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
                    DiscountedPrice = t.DiscountedPrice,
                    DiscountEndDate = t.DiscountEndDate,
                    TotalRooms = t.TotalRooms,
                    AvailableRooms = t.AvailableRooms,
                    PackageType = t.PackageType,
                    AgeLimit = t.AgeLimit,
                    Description = t.Description,
                    MainImageUrl = t.Images.FirstOrDefault()?.ImageUrl
                        ?? "/images/trips/placeholder.jpg"
                }).ToList()
            };

            return View(model);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var trip = await _context.TravelPackages
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            var vm = new TripCardViewModel
            {
                Id = trip.Id,
                Name = trip.Name,
                Destination = trip.Destination,
                Country = trip.Country,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                BasePrice = trip.BasePrice,
                DiscountedPrice = trip.DiscountedPrice,
                DiscountEndDate = trip.DiscountEndDate,
                TotalRooms = trip.TotalRooms,
                AvailableRooms = trip.AvailableRooms,
                PackageType = trip.PackageType,
                AgeLimit = trip.AgeLimit,
                Description = trip.Description,
                MainImageUrl = trip.Images.FirstOrDefault()?.ImageUrl
                    ?? "/images/trips/placeholder.jpg"
            };

            return View(vm);
        }
    }
}
