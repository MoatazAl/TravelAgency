using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly TravelAgencyContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(
        TravelAgencyContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalTrips = await _context.TravelPackages.CountAsync();
        ViewBag.TotalBookings = await _context.Bookings.CountAsync();
        ViewBag.WaitingEntries = await _context.WaitingListEntries.CountAsync();
        ViewBag.TotalUsers = _userManager.Users.Count();

        return View();
    }
}
