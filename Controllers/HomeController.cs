using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;
using System.Security.Claims;


namespace TravelAgency.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TravelAgencyContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            TravelAgencyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
                
            // Load service reviews
            var reviews = await _context.ServiceReviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.ServiceReviews = reviews;
            ViewBag.AverageRating = reviews.Any()
                ? reviews.Average(r => r.Rating)
                : 0;

            // Check if current user already reviewed
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                ViewBag.HasReviewed = await _context.ServiceReviews
                    .AnyAsync(r => r.UserId == userId);
            }
            else
            {
                ViewBag.HasReviewed = false;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
