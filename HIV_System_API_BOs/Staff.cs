using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Staff
{
    public int StfId { get; set; }

    public int AccId { get; set; }

    public string? Degree { get; set; }

    public string? Bio { get; set; }

    public virtual Account Acc { get; set; } = null!;

    public virtual ICollection<ComponentTestResult> ComponentTestResults { get; set; } = new List<ComponentTestResult>();

    public virtual ICollection<SocialBlog> SocialBlogs { get; set; } = new List<SocialBlog>();
}
