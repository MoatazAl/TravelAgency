using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;
using TravelAgency.Models.ViewModels;

namespace TravelAgency.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminReviewsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Trip reviews (linked to trips)
            var tripReviews = await _context.TripReviews
                .Include(r => r.TravelPackage)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Service reviews (platform / service level)
            var serviceReviews = await _context.ServiceReviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Resolve user emails
            var users = _userManager.Users
                .ToDictionary(u => u.Id, u => u.Email);

            ViewBag.TripReviews = tripReviews
                .Select(r => new
                {
                    UserEmail = users.ContainsKey(r.UserId) ? users[r.UserId] : "Unknown",
                    TripName = r.TravelPackage.Name,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToList();

            ViewBag.ServiceReviews = serviceReviews
                .Select(r => new
                {
                    UserEmail = users.ContainsKey(r.UserId) ? users[r.UserId] : "Unknown",
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToList();

            return View();
        }
    }
}
