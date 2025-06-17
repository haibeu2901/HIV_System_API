using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.AccountDTO
{
    public class ChangePasswordRequestDTO
    {
        public string? currentPassword { get; set; }
        public string? newPassword { get; set; }
        public string? confirmNewPassword { get; set; }
    }
}
