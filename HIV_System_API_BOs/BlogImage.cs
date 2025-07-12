using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_BOs;
public partial class BlogImage
{
    public int ImgId { get; set; }

    public int SblId { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual SocialBlog Sbl { get; set; } = null!;
}
