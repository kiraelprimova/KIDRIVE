using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

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

    /// <summary>
    /// Мощность в л.с.
    /// </summary>
    public int? PowerHp { get; set; }

    public int? IdFuelType { get; set; }

    public int? IdTreansmission { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual FuelsType? IdFuelTypeNavigation { get; set; }

    public virtual Transmission? IdTreansmissionNavigation { get; set; }
}
