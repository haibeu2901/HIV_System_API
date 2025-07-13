using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_BOs;

public partial class BlogReaction
{
    public int BrtId { get; set; }

    public int AccId { get; set; }

    public int SblId { get; set; }

    public bool? ReactionType { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReactedAt { get; set; }

    public virtual Account Acc { get; set; } = null!;

    public virtual SocialBlog Sbl { get; set; } = null!;
}
