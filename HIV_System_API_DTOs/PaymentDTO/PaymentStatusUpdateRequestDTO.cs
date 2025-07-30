using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PaymentDTO
{
    public class PaymentStatusUpdateRequestDTO
    {
        public byte Status { get; set; } // 1=Pending, 2=Success, 3=Failed
        public string? Notes { get; set; }
    }
}