using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;

[Authorize(Roles = "Admin")]
public class AdminBookingsController : Controller
{
    private readonly TravelAgencyContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminBookingsController(
        TravelAgencyContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var bookings = await _context.Bookings
            .Include(b => b.TravelPackage)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> WaitingList(int tripId)
    {
        var trip = await _context.TravelPackages.FindAsync(tripId);
        if (trip == null) return NotFound();

        var entries = await _context.WaitingListEntries
            .Where(w => w.TravelPackageId == tripId)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();

        var model = new List<(string Email, DateTime CreatedAt)>();

        foreach (var e in entries)
        {
            var user = await _userManager.FindByIdAsync(e.UserId);
            model.Add((user?.Email ?? "(deleted user)", e.CreatedAt));
        }

        ViewBag.TripName = trip.Name;
        return View(model);
    }



}
