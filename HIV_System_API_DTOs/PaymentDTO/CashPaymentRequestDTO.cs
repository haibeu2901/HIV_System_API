using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PaymentDTO
{
    public class CashPaymentRequestDTO
    {
        public int PmrId { get; set; }
        public int? SrvId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}