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
    [RequireHttps]
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == vm.BookingId);

            if (booking == null || booking.Status != BookingStatus.PendingPayment)
                return BadRequest("Invalid booking state.");

            // Handle PayPal - Redirect to simulated PayPal page
            if (vm.PaymentMethod == "PayPal")
            {
                // Store booking info in TempData for after redirect
                TempData["PayPalBookingId"] = vm.BookingId;
                // Redirect to PayPal simulation page
                return RedirectToAction("PayPalCheckout");
            }

            // Handle Credit Card (existing code)
            if (vm.PaymentMethod == "CreditCard")
            {
                // Validate card fields are present
                if (string.IsNullOrEmpty(vm.CardNumber) || string.IsNullOrEmpty(vm.CVV))
                {
                    ModelState.AddModelError("", "Credit card information is required.");
                    return View("Pay", vm);
                }

                // Expiry validation
                var expiryDate = new DateTime(
                    vm.ExpiryYear,
                    vm.ExpiryMonth,
                    DateTime.DaysInMonth(vm.ExpiryYear, vm.ExpiryMonth));

                if (expiryDate < DateTime.Today)
                {
                    ModelState.AddModelError("", "Card has expired.");
                    return View("Pay", vm);
                }

                // 🔐 Card data intentionally ignored after validation (not stored!)

                var payment = new Payment
                {
                    UserId = user.Id,
                    Amount = booking.TotalPrice,
                    PaymentMethod = "CreditCard",
                    TransactionReference = "CC-" + Guid.NewGuid().ToString(),
                    Status = PaymentStatus.Success
                };

                booking.Status = BookingStatus.Confirmed;
                booking.Payment = payment;

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Payment completed successfully!";
                return RedirectToAction("Index", "Home");
            }

            return BadRequest("Invalid payment method.");
        }

        // GET: Payments/PayPalCheckout - Simulates PayPal redirection
        public IActionResult PayPalCheckout()
        {
            if (TempData["PayPalBookingId"] == null)
                return RedirectToAction("Index", "Home");

            ViewBag.Amount = TempData["PayPalAmount"];
            TempData.Keep("PayPalBookingId"); // Keep for the next request
            TempData.Keep("PayPalAmount");

            return View();
        }

        // POST: Payments/PayPalReturn - Handles return from PayPal
        [HttpPost]
        public async Task<IActionResult> PayPalReturn(
    string status,
    string paypalEmail,
    string paypalPassword)
        {
            // We intentionally ignore paypalEmail and paypalPassword
            // They are NOT validated and NOT stored (simulation only)

            if (TempData["PayPalBookingId"] == null)
                return RedirectToAction("Index", "Home");

            int bookingId = (int)TempData["PayPalBookingId"];

            if (status == "cancel")
            {
                TempData["Warning"] = "PayPal payment was cancelled.";
                return RedirectToAction("Pay", new { bookingId });
            }

            var user = await _userManager.GetUserAsync(User);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Status != BookingStatus.PendingPayment)
                return BadRequest("Invalid booking state.");

            var paypalPayment = new Payment
            {
                UserId = user.Id,
                Amount = booking.TotalPrice,
                PaymentMethod = "PayPal",
                TransactionReference = "PAYPAL-" + Guid.NewGuid(),
                Status = PaymentStatus.Success
            };

            booking.Status = BookingStatus.Confirmed;
            booking.Payment = paypalPayment;

            _context.Payments.Add(paypalPayment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment completed successfully via PayPal!";
            return RedirectToAction("Index", "Home");
        }

    }
}
