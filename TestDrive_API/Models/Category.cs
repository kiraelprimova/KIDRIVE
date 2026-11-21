using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
﻿using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class Category
{
    public int IdCategory { get; set; }

    public string NameCategory { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
