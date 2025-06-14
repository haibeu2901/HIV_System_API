using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.AccountDTO
{
    public class DoctorProfileUpdateDTO
    {
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public DateOnly? Dob { get; set; }
        public bool? Gender { get; set; }
        public string? AccPassword { get; set; }
        public string? Degree { get; set; }
        public string? Bio { get; set; }
    }
}
