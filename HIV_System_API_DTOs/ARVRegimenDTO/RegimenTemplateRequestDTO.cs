using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.ARVRegimenDTO
{
    public class RegimenTemplateRequestDTO
    {
        public string? Description { get; set; }
        public byte? Level { get; set; }
        public int? Duration { get; set; }
    }
}
