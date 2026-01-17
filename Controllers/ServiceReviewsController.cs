using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Data;
using TravelAgency.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelAgency.Controllers
{
    [Authorize]
    public class ServiceReviewsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ServiceReviewsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool alreadyReviewed = await _context.ServiceReviews
                .AnyAsync(r => r.UserId == user.Id);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You have already rated our service.";
                return RedirectToAction("Index", "Home");
            }

            var review = new ServiceReview
            {
                UserId = user.Id,
                Rating = rating,
                Comment = comment ?? ""
            };

            _context.ServiceReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction("Index", "Home");
        }

    }
}
