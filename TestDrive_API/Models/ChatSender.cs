using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class ChatSender
{
    public int IdSender { get; set; }

    public string SenderName { get; set; } = null!;

    public virtual ICollection<AiChatHistory> AiChatHistories { get; set; } = new List<AiChatHistory>();
}
