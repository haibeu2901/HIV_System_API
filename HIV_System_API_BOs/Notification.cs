using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Notification
{
    public int NtfId { get; set; }

    public string? NotiType { get; set; }

    public string? NotiMessage { get; set; }

    public DateTime? SendAt { get; set; }

    public virtual ICollection<NotificationAccount> NotificationAccounts { get; set; } = new List<NotificationAccount>();
}
