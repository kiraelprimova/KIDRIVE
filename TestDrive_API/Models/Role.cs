using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
﻿using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string RoleName { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
