namespace TravelAgency.Models
{
    public class PackageImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = null!;
        public bool IsMain { get; set; } = true;

        public int TravelPackageId { get; set; }
        public TravelPackage TravelPackage { get; set; } = null!;
    }
}
