using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class BookingStatus
{
    public int IdStatus { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
