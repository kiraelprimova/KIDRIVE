using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
﻿using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class ChatSender
{
    public int IdSender { get; set; }

    public string SenderName { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<AiChatHistory> AiChatHistories { get; set; } = new List<AiChatHistory>();
}
