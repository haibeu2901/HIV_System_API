using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Account
{
    public int AccId { get; set; }

    public string AccUsername { get; set; } = null!;

    public string AccPassword { get; set; } = null!;

    public string? Email { get; set; }

    public string? Fullname { get; set; }

    public DateOnly? Dob { get; set; }

    public bool? Gender { get; set; }

    public byte Roles { get; set; }

    public bool? IsActive { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();

    public virtual ICollection<NotificationAccount> NotificationAccounts { get; set; } = new List<NotificationAccount>();

    public virtual Patient? Patient { get; set; }

    public virtual ICollection<SocialBlog> SocialBlogs { get; set; } = new List<SocialBlog>();

    public virtual Staff? Staff { get; set; }
}
