using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientDTO
{
    public class PatientDTO
    {
        public int PtnId { get; set; }
        public int AccId { get; set; }
        public string? AccUsername { get; set; }
        public string? AccPassword { get; set; }
        public string? AccEmail { get; set; }
        public string? FullName { get; set; }
        public DateTime? Dob {  get; set; }
        public bool? Gender { get; set; }
    }
}
