using System;
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

        // =========================
        // GET: /WaitingList/Join
        // =========================
        [HttpGet]
        public async Task<IActionResult> Join(int tripId)
        {
            var trip = await _context.TravelPackages
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return NotFound();

            if (trip.AvailableRooms > 0)
            {
                TempData["Error"] = "This trip is no longer fully booked.";
                return RedirectToAction("Details", "Trips", new { id = tripId });
            }

            if (trip.EndDate < DateTime.UtcNow)
            {
                TempData["Error"] = "This trip has already ended.";
                return RedirectToAction("Index", "Trips");
            }

            ViewBag.WaitingCount = await _context.WaitingListEntries
                .CountAsync(w => w.TravelPackageId == tripId);

            return View(trip);
        }

        // =========================
        // POST: /WaitingList/JoinConfirmed
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinConfirmed(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var trip = await _context.TravelPackages.FindAsync(tripId);
            if (trip == null)
                return NotFound();

            if (trip.AvailableRooms > 0)
            {
                TempData["Error"] = "This trip is no longer fully booked.";
                return RedirectToAction("Details", "Trips", new { id = tripId });
            }

            bool alreadyWaiting = await _context.WaitingListEntries.AnyAsync(w =>
                w.UserId == user.Id &&
                w.TravelPackageId == tripId);

            if (alreadyWaiting)
            {
                TempData["Info"] = "You are already on the waiting list.";
                return RedirectToAction("MyBookings", "Bookings");
            }

            _context.WaitingListEntries.Add(new WaitingListEntry
            {
                UserId = user.Id,
                TravelPackageId = tripId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "You have been added to the waiting list.";
            return RedirectToAction("MyBookings", "Bookings");
        }
    }
}
