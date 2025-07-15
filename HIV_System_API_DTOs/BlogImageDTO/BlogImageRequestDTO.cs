using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.BlogImageDTO
{
    public class BlogImageRequestDTO
    {
        [Required(ErrorMessage = "Social blog ID is required")]
        public int SblId { get; set; }
        [Required(ErrorMessage = "Image file is required")]
        public IFormFile? Image { get; set; } // Add this property for image upload
    }
}
