using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.AccountDTO
{
    public class BasicProfileResponseDTO
    {
        public int AccId { get; set; }
        public string? AccUsername { get; set; }
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public DateOnly? Dob { get; set; }
        public bool? Gender { get; set; }
        public int Roles { get; set; }
        public bool? IsActive { get; set; }
    }
}
