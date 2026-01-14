using System;
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
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly TravelAgencyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PaymentsController(
            TravelAgencyContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Payments/Pay?bookingId=5
        public async Task<IActionResult> Pay(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return NotFound();

            if (booking.Status == BookingStatus.Confirmed)
                return BadRequest("This booking has already been paid.");

            var vm = new PaymentViewModel
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                TripName = booking.TravelPackage.Name
            };

            return View(vm);
        }

        // POST: Payments/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(PaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Pay", vm);

            // Expiry validation (server-side)
            var expiryDate = new DateTime(
                vm.ExpiryYear,
                vm.ExpiryMonth,
                DateTime.DaysInMonth(vm.ExpiryYear, vm.ExpiryMonth));

            if (expiryDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Card has expired.");
                return View("Pay", vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == vm.BookingId);

            if (booking == null || booking.Status != BookingStatus.PendingPayment)
                return BadRequest("Invalid booking state.");

            // 🔐 Card data intentionally ignored after validation

            var payment = new Payment
            {
                UserId = user.Id,
                Amount = booking.TotalPrice,
                PaymentMethod = "CreditCard",
                TransactionReference = Guid.NewGuid().ToString(),
                Status = PaymentStatus.Success
            };

            booking.Status = BookingStatus.Confirmed;
            booking.Payment = payment;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment completed successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
