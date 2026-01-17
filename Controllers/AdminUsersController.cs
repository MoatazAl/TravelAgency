using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models.ViewModels;


namespace TravelAgency.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TravelAgencyContext _context;

        public AdminUsersController(
            UserManager<IdentityUser> userManager,
            TravelAgencyContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // =========================
        // USERS LIST
        // =========================
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var model = users.Select(u => new AdminUserViewModel
            {
                UserId = u.Id,
                Email = u.Email!,
                IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow,
                BookingCount = _context.Bookings.Count(b => b.UserId == u.Id)
            }).ToList();

            return View(model);
        }

        // =========================
        // TOGGLE USER STATUS
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // UNLOCK
                user.LockoutEnd = null;
            }
            else
            {
                // LOCK (10 years = effectively disabled)
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(10);
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // USER BOOKINGS
        // =========================
        public async Task<IActionResult> Bookings(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var bookings = await _context.Bookings
                .Include(b => b.TravelPackage)
                .Where(b => b.UserId == id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            ViewBag.UserEmail = user.Email;
            return View(bookings);
        }

        // =========================
        // DELETE USER (CONFIRM)
        // =========================
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var bookingCount = await _context.Bookings
                .CountAsync(b => b.UserId == id);

            ViewBag.BookingCount = bookingCount;

            return View(user);
        }

        // =========================
        // DELETE USER (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            bool hasBookings = await _context.Bookings
                .AnyAsync(b => b.UserId == id);

            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete user with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to delete user.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
