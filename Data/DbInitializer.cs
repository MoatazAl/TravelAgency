using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Models;

namespace TravelAgency.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TravelAgencyContext context)
        {
            context.Database.Migrate();

            if (context.TravelPackages.Any())
                return;

            var trips = new[]
            {
                new TravelPackage
                {
                    Name = "Paris Romantic Getaway",
                    Destination = "Paris",
                    Country = "France",
                    StartDate = new DateTime(2026, 4, 10),
                    EndDate = new DateTime(2026, 4, 16),
                    BasePrice = 1450,
                    DiscountedPrice = 1300,
                    DiscountEndDate = new DateTime(2026, 4, 1),
                    TotalRooms = 10,
                    AvailableRooms = 10,
                    PackageType = "Honeymoon",
                    AgeLimit = 18,
                    Description = "Eiffel Tower views, Seine River cruise, luxury 4-star hotel.",
                    Images = { new PackageImage { ImageUrl = "/images/paris_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Maldives Overwater Villas",
                    Destination = "Male",
                    Country = "Maldives",
                    StartDate = new DateTime(2026, 6, 3),
                    EndDate = new DateTime(2026, 6, 10),
                    BasePrice = 3200,
                    DiscountedPrice = 2900,
                    DiscountEndDate = new DateTime(2026, 5, 20),
                    TotalRooms = 8,
                    AvailableRooms = 8,
                    PackageType = "Luxury",
                    AgeLimit = 18,
                    Description = "Overwater bungalows, private pools, snorkeling experience.",
                    Images = { new PackageImage { ImageUrl = "/images/maldives_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Dubai Family Summer Package",
                    Destination = "Dubai",
                    Country = "UAE",
                    StartDate = new DateTime(2026, 7, 15),
                    EndDate = new DateTime(2026, 7, 22),
                    BasePrice = 2100,
                    TotalRooms = 20,
                    AvailableRooms = 20,
                    PackageType = "Family",
                    Description = "Atlantis waterpark, desert safari, Burj Khalifa ticket.",
                    Images = { new PackageImage { ImageUrl = "/images/dubai_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Swiss Alps Adventure",
                    Destination = "Interlaken",
                    Country = "Switzerland",
                    StartDate = new DateTime(2026, 2, 10),
                    EndDate = new DateTime(2026, 2, 17),
                    BasePrice = 1800,
                    TotalRooms = 12,
                    AvailableRooms = 12,
                    PackageType = "Adventure",
                    AgeLimit = 16,
                    Description = "Paragliding, snow hikes, Jungfraujoch railway.",
                    Images = { new PackageImage { ImageUrl = "/images/swiss_alps_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Tokyo Cherry Blossom Tour",
                    Destination = "Tokyo",
                    Country = "Japan",
                    StartDate = new DateTime(2026, 3, 20),
                    EndDate = new DateTime(2026, 3, 27),
                    BasePrice = 2300,
                    TotalRooms = 15,
                    AvailableRooms = 15,
                    PackageType = "Cultural",
                    Description = "Sakura viewing, Shibuya exploration, Mt. Fuji day trip.",
                    Images = { new PackageImage { ImageUrl = "/images/tokyo_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Thailand Beach Escape",
                    Destination = "Phuket",
                    Country = "Thailand",
                    StartDate = new DateTime(2026, 8, 10),
                    EndDate = new DateTime(2026, 8, 17),
                    BasePrice = 1400,
                    TotalRooms = 18,
                    AvailableRooms = 18,
                    PackageType = "Family",
                    Description = "Island hopping, pink beaches, elephant sanctuary visit.",
                    Images = { new PackageImage { ImageUrl = "/images/phuket_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Santorini Blue Domes Honeymoon",
                    Destination = "Santorini",
                    Country = "Greece",
                    StartDate = new DateTime(2026, 5, 1),
                    EndDate = new DateTime(2026, 5, 8),
                    BasePrice = 2600,
                    DiscountedPrice = 2400,
                    DiscountEndDate = new DateTime(2026, 4, 20),
                    TotalRooms = 10,
                    AvailableRooms = 10,
                    PackageType = "Honeymoon",
                    AgeLimit = 18,
                    Description = "Private hot-tub suites, sunset cruise, wine tasting.",
                    Images = { new PackageImage { ImageUrl = "/images/santorini_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Nile River Cruise",
                    Destination = "Luxor",
                    Country = "Egypt",
                    StartDate = new DateTime(2026, 11, 12),
                    EndDate = new DateTime(2026, 11, 20),
                    BasePrice = 1700,
                    TotalRooms = 25,
                    AvailableRooms = 25,
                    PackageType = "Cruise",
                    Description = "Luxor temples, Valley of the Kings, luxury Nile cruise.",
                    Images = { new PackageImage { ImageUrl = "/images/nile_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "New York City Winter Lights",
                    Destination = "New York",
                    Country = "USA",
                    StartDate = new DateTime(2026, 12, 18),
                    EndDate = new DateTime(2026, 12, 25),
                    BasePrice = 2500,
                    TotalRooms = 14,
                    AvailableRooms = 14,
                    PackageType = "City Break",
                    Description = "Times Square, Rockefeller tree, Broadway show.",
                    Images = { new PackageImage { ImageUrl = "/images/new_york_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Venice Romantic Canals",
                    Destination = "Venice",
                    Country = "Italy",
                    StartDate = new DateTime(2026, 4, 5),
                    EndDate = new DateTime(2026, 4, 10),
                    BasePrice = 1600,
                    TotalRooms = 10,
                    AvailableRooms = 10,
                    PackageType = "Honeymoon",
                    Description = "Gondola rides, St. Mark’s Basilica, romantic old town.",
                    Images = { new PackageImage { ImageUrl = "/images/venice_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Barcelona Summer Vibes",
                    Destination = "Barcelona",
                    Country = "Spain",
                    StartDate = new DateTime(2026, 6, 12),
                    EndDate = new DateTime(2026, 6, 19),
                    BasePrice = 1500,
                    TotalRooms = 17,
                    AvailableRooms = 17,
                    PackageType = "Family",
                    Description = "Beach days, Sagrada Familia, Camp Nou tour.",
                    Images = { new PackageImage { ImageUrl = "/images/barcelona_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Rio de Janeiro Carnival Experience",
                    Destination = "Rio de Janeiro",
                    Country = "Brazil",
                    StartDate = new DateTime(2027, 2, 15),
                    EndDate = new DateTime(2027, 2, 22),
                    BasePrice = 2800,
                    TotalRooms = 15,
                    AvailableRooms = 15,
                    PackageType = "Cultural",
                    Description = "Carnival parade, Sugarloaf Mountain, Copacabana beach.",
                    Images = { new PackageImage { ImageUrl = "/images/rio_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "South Africa Safari",
                    Destination = "Kruger National Park",
                    Country = "South Africa",
                    StartDate = new DateTime(2026, 9, 5),
                    EndDate = new DateTime(2026, 9, 12),
                    BasePrice = 3000,
                    TotalRooms = 8,
                    AvailableRooms = 8,
                    PackageType = "Adventure",
                    AgeLimit = 12,
                    Description = "Big Five safari, luxury lodge, wildlife tours.",
                    Images = { new PackageImage { ImageUrl = "/images/kruger_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Sydney City Break",
                    Destination = "Sydney",
                    Country = "Australia",
                    StartDate = new DateTime(2026, 10, 10),
                    EndDate = new DateTime(2026, 10, 17),
                    BasePrice = 2600,
                    TotalRooms = 12,
                    AvailableRooms = 12,
                    PackageType = "City Break",
                    Description = "Sydney Opera House, harbour cruise, Bondi Beach.",
                    Images = { new PackageImage { ImageUrl = "/images/sydney_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Portugal Coastal Drive",
                    Destination = "Algarve",
                    Country = "Portugal",
                    StartDate = new DateTime(2026, 5, 20),
                    EndDate = new DateTime(2026, 5, 28),
                    BasePrice = 1800,
                    TotalRooms = 16,
                    AvailableRooms = 16,
                    PackageType = "Family",
                    Description = "Cliff beaches, boat caves, traditional cuisine.",
                    Images = { new PackageImage { ImageUrl = "/images/algarve_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Seoul Tech & Culture Tour",
                    Destination = "Seoul",
                    Country = "South Korea",
                    StartDate = new DateTime(2026, 10, 1),
                    EndDate = new DateTime(2026, 10, 7),
                    BasePrice = 2100,
                    TotalRooms = 15,
                    AvailableRooms = 15,
                    PackageType = "Cultural",
                    Description = "Gyeongbokgung Palace, K-pop museum, Korean BBQ.",
                    Images = { new PackageImage { ImageUrl = "/images/seoul_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Cancun All-Inclusive Resort",
                    Destination = "Cancun",
                    Country = "Mexico",
                    StartDate = new DateTime(2026, 7, 1),
                    EndDate = new DateTime(2026, 7, 10),
                    BasePrice = 2200,
                    TotalRooms = 20,
                    AvailableRooms = 20,
                    PackageType = "Luxury",
                    Description = "Beachfront resort, all meals included, water sports.",
                    Images = { new PackageImage { ImageUrl = "/images/cancun_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Istanbul Cultural Heritage",
                    Destination = "Istanbul",
                    Country = "Turkey",
                    StartDate = new DateTime(2026, 4, 14),
                    EndDate = new DateTime(2026, 4, 20),
                    BasePrice = 1300,
                    TotalRooms = 22,
                    AvailableRooms = 22,
                    PackageType = "Cultural",
                    Description = "Hagia Sophia, Bosphorus cruise, Grand Bazaar.",
                    Images = { new PackageImage { ImageUrl = "/images/istanbul_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Canadian Rockies",
                    Destination = "Banff",
                    Country = "Canada",
                    StartDate = new DateTime(2026, 9, 14),
                    EndDate = new DateTime(2026, 9, 22),
                    BasePrice = 2400,
                    TotalRooms = 12,
                    AvailableRooms = 12,
                    PackageType = "Adventure",
                    Description = "Lake Louise, mountain hikes, scenic drives.",
                    Images = { new PackageImage { ImageUrl = "/images/banff_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Bali Yoga & Wellness Retreat",
                    Destination = "Ubud",
                    Country = "Indonesia",
                    StartDate = new DateTime(2026, 5, 1),
                    EndDate = new DateTime(2026, 5, 10),
                    BasePrice = 1750,
                    TotalRooms = 14,
                    AvailableRooms = 14,
                    PackageType = "Wellness",
                    AgeLimit = 16,
                    Description = "Yoga sessions, spa treatments, rice terraces.",
                    Images = { new PackageImage { ImageUrl = "/images/bali_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Vienna Classical Music Tour",
                    Destination = "Vienna",
                    Country = "Austria",
                    StartDate = new DateTime(2026, 12, 1),
                    EndDate = new DateTime(2026, 12, 7),
                    BasePrice = 1600,
                    TotalRooms = 10,
                    AvailableRooms = 10,
                    PackageType = "Cultural",
                    Description = "Opera night, palace tours, Christmas markets.",
                    Images = { new PackageImage { ImageUrl = "/images/vienna_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "Norway Northern Lights",
                    Destination = "Tromsø",
                    Country = "Norway",
                    StartDate = new DateTime(2026, 1, 9),
                    EndDate = new DateTime(2026, 1, 15),
                    BasePrice = 2800,
                    TotalRooms = 10,
                    AvailableRooms = 10,
                    PackageType = "Adventure",
                    Description = "Aurora hunting, snow cabins, polar tours.",
                    Images = { new PackageImage { ImageUrl = "/images/tromso_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "India Golden Triangle",
                    Destination = "Delhi, Agra, Jaipur",
                    Country = "India",
                    StartDate = new DateTime(2026, 11, 5),
                    EndDate = new DateTime(2026, 11, 15),
                    BasePrice = 1700,
                    TotalRooms = 18,
                    AvailableRooms = 18,
                    PackageType = "Cultural",
                    Description = "Taj Mahal, Jaipur palaces, guided tours.",
                    Images =
                    {
                        new PackageImage { ImageUrl = "/images/india_triangle_01.jpg" },
                        new PackageImage { ImageUrl = "/images/india_triangle_02.jpg" },
                        new PackageImage { ImageUrl = "/images/india_triangle_03.jpg" }
                    }
                },

                new TravelPackage
                {
                    Name = "Finland Lapland Winter Wonderland",
                    Destination = "Rovaniemi",
                    Country = "Finland",
                    StartDate = new DateTime(2026, 12, 20),
                    EndDate = new DateTime(2026, 12, 27),
                    BasePrice = 2600,
                    TotalRooms = 12,
                    AvailableRooms = 12,
                    PackageType = "Winter",
                    Description = "Husky rides, reindeer safari, Santa village visit.",
                    Images = { new PackageImage { ImageUrl = "/images/lapland_01.jpg" } }
                },

                new TravelPackage
                {
                    Name = "New Zealand Scenic Road Trip",
                    Destination = "Queenstown",
                    Country = "New Zealand",
                    StartDate = new DateTime(2026, 10, 15),
                    EndDate = new DateTime(2026, 10, 25),
                    BasePrice = 2900,
                    TotalRooms = 12,
                    AvailableRooms = 12,
                    PackageType = "Adventure",
                    Description = "Milford Sound, Hobbiton, glacier valleys.",
                    Images = { new PackageImage { ImageUrl = "/images/queenstown_01.jpg" } }
                }
            };

            context.TravelPackages.AddRange(trips);
            context.SaveChanges();
        }
    }
}
