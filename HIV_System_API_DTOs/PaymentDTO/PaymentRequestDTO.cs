using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PaymentDTO
{
    public class PaymentRequestDTO
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
    }
}
