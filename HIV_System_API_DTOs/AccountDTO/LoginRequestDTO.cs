using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.AccountDTO
{
    public class LoginRequestDTO
    {
        public string? AccUsername { get; set; }
        public string? AccPassword { get; set; }
    }
}
