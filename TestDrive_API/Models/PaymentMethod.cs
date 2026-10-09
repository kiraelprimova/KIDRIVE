using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class PaymentMethod
{
    public int IdPaymentMethod { get; set; }

    public string MethodName { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
