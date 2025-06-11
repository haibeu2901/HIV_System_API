using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class StaffDTO
    {
        public int StfId { get; set; }

        public int AccId { get; set; }

        public string? Degree { get; set; }

        public string? Bio { get; set; } = null;
    }
}
