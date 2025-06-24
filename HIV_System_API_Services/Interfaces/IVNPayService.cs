using HIV_System_API_DTOs.VNPayPaymentDTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(VNPayPaymentRequestDTO request, string clientIp);
        bool ValidateSignature(IQueryCollection query);
    }
}
