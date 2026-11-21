using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class Booking
{
    public int IdBooking { get; set; }

    public int IdUser { get; set; }

    public int IdCar { get; set; }

    public DateOnly BookingDate { get; set; }

    public TimeOnly BookingTime { get; set; }

    public int IdStatus { get; set; }

    public int IdPayment { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual Car? IdCarNavigation { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual PaymentMethod? IdPaymentNavigation { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual BookingStatus? IdStatusNavigation { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual User? IdUserNavigation { get; set; }
}
