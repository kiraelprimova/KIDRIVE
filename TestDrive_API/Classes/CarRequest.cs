namespace TestDrive_API.Classes;

public class CarRequest
{
    public int IdCategory { get; set; }
    public string Model { get; set; } = null!;
    public short YearCar { get; set; }
    public string Description { get; set; } = null!;
    public string ImagePath { get; set; } = null!;
    public decimal Price { get; set; }
    public int? PowerHp { get; set; }
    public int? IdFuelType { get; set; }
    public int? IdTreansmission { get; set; }
}
