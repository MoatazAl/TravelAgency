using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;

namespace TravelAgency.Controllers
{
    [Authorize]
    public class WaitingListController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public WaitingListController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /WaitingList/Join?tripId=5
        public async Task<IActionResult> Join(int tripId)
        {
            var trip = await _context.TravelPackages
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return NotFound();

            // If rooms exist → do NOT allow waiting list
            if (trip.AvailableRooms > 0)
            {
                return RedirectToAction("Create", "Bookings", new { tripId });
            }

            int count = await _context.WaitingListEntries
                .CountAsync(w => w.TravelPackageId == tripId);

            ViewBag.TripId = tripId;
            ViewBag.TripName = trip.Name;
            ViewBag.WaitingCount = count;

            // Simple ETA for PDF requirement
            ViewBag.EstimatedDays = (count + 1) * 2;

            return View();
        }

        // POST: /WaitingList/JoinConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinConfirmed(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            bool alreadyWaiting = await _context.WaitingListEntries.AnyAsync(w =>
                w.UserId == user.Id &&
                w.TravelPackageId == tripId);

            if (alreadyWaiting)
            {
                TempData["Error"] = "You are already on the waiting list.";
                return RedirectToAction("MyBookings", "Bookings");
            }

            int position = await _context.WaitingListEntries
                .CountAsync(w => w.TravelPackageId == tripId) + 1;

            var entry = new WaitingListEntry
            {
                UserId = user.Id,
                TravelPackageId = tripId,
                Position = position,
                CreatedAt = DateTime.UtcNow
            };

            _context.WaitingListEntries.Add(entry);
            await _context.SaveChangesAsync();

            TempData["Success"] = "You joined the waiting list.";
            return RedirectToAction("MyBookings", "Bookings");
        }
    }
}
