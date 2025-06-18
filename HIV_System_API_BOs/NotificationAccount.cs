using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class NotificationAccount
{
    public int NtaId { get; set; }

    public int AccId { get; set; }

    public int NtfId { get; set; }
    public bool IsRead { get; set; }

    public virtual Account Acc { get; set; } = null!;

    public virtual Notification Ntf { get; set; } = null!;
}
