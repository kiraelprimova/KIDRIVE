using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class Favorite
{
    public int IdFavorite { get; set; }

    public int IdUser { get; set; }

    public int IdCar { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Car IdCarNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
