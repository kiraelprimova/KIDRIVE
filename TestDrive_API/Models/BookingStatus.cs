using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
﻿using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class BookingStatus
{
    public int IdStatus { get; set; }

    public string StatusName { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
