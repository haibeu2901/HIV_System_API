using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class SocialBlog
{
    public int SblId { get; set; }

    public int AccId { get; set; }

    public int? StfId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public bool? IsAnonymous { get; set; }

    public string? Notes { get; set; }

    public byte BlogStatus { get; set; }

    public DateTime? PublishedAt { get; set; }

    public virtual Account Acc { get; set; } = null!;

    public virtual Staff? Stf { get; set; }
}
