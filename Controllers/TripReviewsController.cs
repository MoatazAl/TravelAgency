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
    public class TripReviewsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TripReviewsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId, int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            bool hasBooked = await _context.Bookings.AnyAsync(b =>
                b.UserId == user.Id &&
                b.TravelPackageId == tripId &&
                b.Status != BookingStatus.Cancelled);

            if (!hasBooked)
                return Forbid();

            bool alreadyReviewed = await _context.TripReviews.AnyAsync(r =>
                r.UserId == user.Id &&
                r.TravelPackageId == tripId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You already reviewed this trip.";
                return RedirectToAction("Details", "Trips", new { id = tripId });
            }

            var review = new TripReview
            {
                UserId = user.Id,
                TravelPackageId = tripId,
                Rating = rating,
                Comment = comment ?? ""
            };

            _context.TripReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("Details", "Trips", new { id = tripId });
        }
    }
}
