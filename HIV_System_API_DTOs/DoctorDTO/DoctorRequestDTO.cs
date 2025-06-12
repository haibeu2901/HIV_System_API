using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.DoctorDTO
{
    public class DoctorRequestDTO
    {
        public int AccId { get; set; }
        public string? Degree { get; set; }
        public string? Bio {  get; set; }
    }
}
