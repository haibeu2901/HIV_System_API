using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IVerificationCodeService
    {
        string GenerateCode(string email);
        bool VerifyCode(string email, string code);
    }
}
