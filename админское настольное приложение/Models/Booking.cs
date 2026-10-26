using System.Text.Json.Serialization;
using TestDriveAPI.Models;

public partial class Booking
{
    public int IdBooking { get; set; }

    public int IdUser { get; set; }

    public int IdCar { get; set; }

    public DateOnly BookingDate { get; set; }

    public TimeOnly BookingTime { get; set; }

    public int IdStatus { get; set; }

    [JsonIgnore]
    public virtual Car? IdCarNavigation { get; set; }

    [JsonIgnore]
    public virtual BookingStatus? IdStatusNavigation { get; set; }

    [JsonIgnore]
    public virtual User? IdUserNavigation { get; set; }
}