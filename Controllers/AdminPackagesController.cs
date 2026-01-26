using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using TravelAgency.Models;

[Authorize(Roles = "Admin")]
public class AdminPackagesController : Controller
{
    private readonly TravelAgencyContext _context;

    public AdminPackagesController(TravelAgencyContext context)
    {
        _context = context;
    }

    // =========================
    // LIST
    // =========================
    public async Task<IActionResult> Index()
    {
        var packages = await _context.TravelPackages
            .Include(p => p.Images)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        return View(packages);
    }

    // =========================
    // CREATE (GET)
    // =========================
    public IActionResult Create()
    {
        return View();
    }

    // =========================
    // CREATE (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TravelPackage model, string imageUrls)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Rooms logic
        model.AvailableRooms = model.TotalRooms;

        // 🔒 Discount validation
        if (model.DiscountedPrice != null && model.DiscountEndDate == null)
        {
            ModelState.AddModelError("", "Discount end date is required when setting a discounted price.");
            return View(model);
        }

        if (model.DiscountedPrice == null)
        {
            model.DiscountEndDate = null;
        }

        // Images
        if (!string.IsNullOrWhiteSpace(imageUrls))
        {
            var urls = imageUrls
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(u => u.Trim())
                .ToList();

            for (int i = 0; i < urls.Count; i++)
            {
                model.Images.Add(new PackageImage
                {
                    ImageUrl = urls[i],
                    IsMain = (i == 0)
                });
            }
        }

        // Fallback image
        if (!model.Images.Any())
        {
            model.Images.Add(new PackageImage
            {
                ImageUrl = "/images/trips/placeholder.jpg",
                IsMain = true
            });
        }

        _context.TravelPackages.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Package created successfully.";
        return RedirectToAction(nameof(Index));
    }


    // =========================
    // EDIT (GET)
    // =========================
    public async Task<IActionResult> Edit(int id)
    {
        var package = await _context.TravelPackages
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
            return NotFound();

        return View(package);
    }

    // =========================
    // EDIT (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TravelPackage updated)
    {
        var package = await _context.TravelPackages
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
            return NotFound();

        // 🧠 Calculate booked rooms BEFORE update
        int bookedRooms = package.TotalRooms - package.AvailableRooms;

        // store old availability to detect increase
        int oldAvailable = package.AvailableRooms;

        // ✅ Update editable fields
        package.Name = updated.Name;
        package.Destination = updated.Destination;
        package.Country = updated.Country;
        package.StartDate = updated.StartDate;
        package.EndDate = updated.EndDate;
        package.BasePrice = updated.BasePrice;
        package.DiscountedPrice = updated.DiscountedPrice;
        package.DiscountEndDate = updated.DiscountEndDate;
        package.PackageType = updated.PackageType;
        package.AgeLimit = updated.AgeLimit;
        package.Description = updated.Description;
        package.BookingDeadline = updated.BookingDeadline;
        package.TotalRooms = updated.TotalRooms;

        // 🔥 Recalculate availability correctly
        package.AvailableRooms = Math.Max(
            0,
            updated.TotalRooms - bookedRooms
        );

        await _context.SaveChangesAsync();

        // =====================================================
        // ✅ AUTO-PROMOTE WAITING LIST IF ROOMS INCREASED
        // =====================================================
        if (package.AvailableRooms > oldAvailable)
        {
            var waitingList = await _context.WaitingListEntries
                .Where(w => w.TravelPackageId == package.Id)
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

            while (package.AvailableRooms > 0 && waitingList.Count > 0)
            {
                var entry = waitingList[0];

                _context.Bookings.Add(new Booking
                {
                    UserId = entry.UserId,
                    TravelPackageId = package.Id,
                    TotalPrice = package.DiscountedPrice ?? package.BasePrice,
                    BookingDate = DateTime.UtcNow,
                    DepartureDate = package.StartDate,
                    CancellationAllowedUntil = package.StartDate.AddDays(-7),
                    Status = BookingStatus.PendingPayment 
                });


                _context.WaitingListEntries.Remove(entry);
                package.AvailableRooms--;

                waitingList.RemoveAt(0);
            }

            await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Package updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================
    // TOGGLE VISIBILITY
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleVisibility(int id)
    {
        var package = await _context.TravelPackages.FindAsync(id);

        if (package == null)
            return NotFound();

        package.IsVisible = !package.IsVisible;

        await _context.SaveChangesAsync();

        TempData["Success"] = package.IsVisible
            ? "Package is now visible to users."
            : "Package has been hidden from users.";

        return RedirectToAction(nameof(Index));
    }


    // =========================
    // DELETE (GET)
    // =========================
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _context.TravelPackages
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
            return NotFound();

        return View(package);
    }

    // =========================
    // DELETE (POST)
    // =========================
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var package = await _context.TravelPackages
            .Include(p => p.Images)
            .Include(p => p.Bookings)
            .Include(p => p.WaitingList)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
            return NotFound();

        // SAFETY RULE:
        // Do NOT allow deleting packages with bookings
        if (package.Bookings.Any(b => b.Status != BookingStatus.Cancelled))
        {
            TempData["Error"] = "Cannot delete a package that has active bookings.";
            return RedirectToAction(nameof(Index));
        }

        // Remove related data explicitly (safe)
        _context.WaitingListEntries.RemoveRange(package.WaitingList);
        _context.PackageImages.RemoveRange(package.Images);
        _context.TravelPackages.Remove(package);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Package deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

}
