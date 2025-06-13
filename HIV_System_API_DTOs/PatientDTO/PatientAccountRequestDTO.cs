using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientDTO
{
    public class PatientAccountRequestDTO
    {
        public string? AccUsername { get; set; }
        public string? AccPassword { get; set; }
        public string? Email {  get; set; }
        public string? Fullname { get; set; }
        public DateTime? Dob {  get; set; }
        public bool? Gender {  get; set; }
    }
}
