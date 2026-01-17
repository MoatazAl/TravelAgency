using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelAgency.Models;

public static class ItineraryPdfService
{
    public static byte[] Generate(Booking booking)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12));

                // =========================
                // HEADER
                // =========================
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("NoorAgency")
                            .FontSize(22)
                            .SemiBold()
                            .FontColor(Colors.Red.Darken2);

                        col.Item().Text("Official Travel Itinerary")
                            .FontSize(14)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });

                // =========================
                // CONTENT
                // =========================
                page.Content().PaddingVertical(25).Column(col =>
                {
                    col.Spacing(15);

                    // Trip Title
                    col.Item().Text(booking.TravelPackage.Name)
                        .FontSize(20)
                        .SemiBold();

                    col.Item().Text($"{booking.TravelPackage.Destination}, {booking.TravelPackage.Country}")
                        .FontSize(14)
                        .FontColor(Colors.Grey.Darken1);

                    // Divider
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Booking Info Box
                    col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(box =>
                    {
                        box.Spacing(8);

                        InfoRow(box, "Booking ID:", booking.Id.ToString());
                        InfoRow(box, "Departure Date:", booking.DepartureDate.ToString("dd MMM yyyy"));
                        InfoRow(box, "Return Date:", booking.TravelPackage.EndDate.ToString("dd MMM yyyy"));
                        InfoRow(box, "Status:", booking.Status.ToString());
                    });

                    // Price Highlight
                    col.Item().AlignRight().Text(text =>
                    {
                        text.Span("Total Paid: ").FontSize(14);
                        text.Span($"€{booking.TotalPrice}")
                            .FontSize(18)
                            .SemiBold()
                            .FontColor(Colors.Green.Darken1);
                    });

                    // Message
                    col.Item().PaddingTop(20).Text(
                        "Thank you for choosing NoorAgency. " +
                        "Please keep this itinerary with you during your trip.")
                        .FontColor(Colors.Grey.Darken1);
                });

                // =========================
                // FOOTER
                // =========================
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ");
                    text.Span(DateTime.UtcNow.ToString("dd MMM yyyy"));
                    text.Span(" • NoorAgency");
                });
            });
        }).GeneratePdf();
    }

    private static void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(120).Text(label).SemiBold();
            row.RelativeItem().Text(value);
        });
    }
}
