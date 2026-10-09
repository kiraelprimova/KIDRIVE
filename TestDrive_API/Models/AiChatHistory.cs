using System;
using System.Collections.Generic;

namespace TestDrive_API.Models;

public partial class AiChatHistory
{
    public int IdMessage { get; set; }

    public int IdUser { get; set; }

    public int IdSender { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ChatSender IdSenderNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
