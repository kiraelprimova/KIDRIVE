namespace TestDrive_API.Classes
{
    public class BookingDTO
    {
        public int IdCar { get; set; }
        public int IdUser { get; set; }
        public DateOnly BookingDate { get; set; }
        public TimeOnly BookingTime { get; set; }
        public int IdPayment { get; set; } = 1;
    }
}
