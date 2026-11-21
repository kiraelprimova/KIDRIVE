using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
﻿using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class AiChatHistory
{
    public int IdMessage { get; set; }

    public int IdUser { get; set; }

    public int IdSender { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual ChatSender IdSenderNavigation { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual User IdUserNavigation { get; set; } = null!;
}
