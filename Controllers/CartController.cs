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
    public class CartController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(TravelAgencyContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var items = await _context.CartItems
                .Include(c => c.TravelPackage)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var exists = await _context.CartItems.AnyAsync(c =>
                c.UserId == user.Id && c.TravelPackageId == tripId);

            if (!exists)
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = user.Id,
                    TravelPackageId = tripId
                });
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Added to cart.";
            return RedirectToAction("Index", "Trips");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Checkout: create bookings (PendingPayment) for all cart items, then redirect to MyBookings (user can pay each)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cartItems = await _context.CartItems
                .Include(c => c.TravelPackage)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in cartItems)
            {
                // reuse same rules as bookings: no duplicate active booking
                var already = await _context.Bookings.AnyAsync(b =>
                    b.UserId == user.Id &&
                    b.TravelPackageId == item.TravelPackageId &&
                    b.Status != BookingStatus.Cancelled);

                if (already) continue;

                if (item.TravelPackage.AvailableRooms <= 0) continue;

                // reserve room + create booking
                item.TravelPackage.AvailableRooms -= 1;

                _context.Bookings.Add(new Booking
                {
                    UserId = user.Id,
                    TravelPackageId = item.TravelPackageId,
                    TotalPrice = item.TravelPackage.DiscountedPrice ?? item.TravelPackage.BasePrice,
                    DepartureDate = item.TravelPackage.StartDate,
                    CancellationAllowedUntil = item.TravelPackage.StartDate.AddDays(-7),
                    Status = BookingStatus.PendingPayment
                });
            }

            // clear cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Checkout created bookings. Please complete payment from My Bookings.";
            return RedirectToAction("MyBookings", "Bookings");
        }
    }
}
