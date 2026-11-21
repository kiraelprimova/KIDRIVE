using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
﻿using System;
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

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [JsonIgnore]
    [ValidateNever]
    public virtual Category IdCategoryNavigation { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual FuelsType? IdFuelTypeNavigation { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual Transmission? IdTreansmissionNavigation { get; set; }
}
