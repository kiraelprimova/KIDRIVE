using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestDriveAPI.Models;

public partial class Car
{
    public int IdCar { get; set; }

    public int IdCategory { get; set; }

    public string Model { get; set; } = null!;

    public short YearCar { get; set; }

    public string Description { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public sbyte IsDeleted { get; set; }
    public decimal Price { get; set; }

    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    [JsonIgnore]
    public virtual Category IdCategoryNavigation { get; set; } = null!;
}
