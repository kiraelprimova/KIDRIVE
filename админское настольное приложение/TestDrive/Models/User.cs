using System;
using System.Collections.Generic;

namespace TestDriveAPI.Models;

public partial class User
{
    public int IdUser { get; set; }

    public int IdRole { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public sbyte IsDelete { get; set; }

    public int Account { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
