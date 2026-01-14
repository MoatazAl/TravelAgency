using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelAgency.Models;

namespace TravelAgency.Services
{
    public static class ItineraryPdfService
    {
        public static byte[] Generate(Booking booking)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Text("Travel Itinerary")
                        .FontSize(22)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Trip: {booking.TravelPackage.Name}").Bold();
                        col.Item().Text($"Destination: {booking.TravelPackage.Destination}, {booking.TravelPackage.Country}");
                        col.Item().Text($"Departure: {booking.DepartureDate:dd MMM yyyy}");
                        col.Item().Text($"Price Paid: €{booking.TotalPrice}");
                        col.Item().Text($"Booking ID: {booking.Id}");
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("© 2025 TravelAgency");
                });
            }).GeneratePdf();
        }
    }
}
