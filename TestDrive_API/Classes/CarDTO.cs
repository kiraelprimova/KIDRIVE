namespace TestDrive_API.Classes
{
    public class CarDTO
    {
        public int IdCategory { get; set; }

        public string Model { get; set; }

        public int YearCar { get; set; }

        public string Description { get; set; }

        public string ImagePath { get; set; }

        public decimal Price { get; set; }
        public int? PowerHp { get; set; }
        public int? IdFuelType { get; set; }
        public int? IdTreansmission { get; set; }
    }
}
