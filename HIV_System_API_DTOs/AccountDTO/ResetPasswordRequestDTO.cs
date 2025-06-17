using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.AccountDTO
{
    public class ResetPasswordRequestDTO
    {
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
