using System;
using System.Collections.Generic;

namespace TestDriveAPI.Models;

public partial class Category
{
    public int IdCategory { get; set; }

    public string NameCategory { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
