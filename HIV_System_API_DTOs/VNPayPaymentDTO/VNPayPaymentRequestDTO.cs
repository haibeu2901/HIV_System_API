using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.VNPayPaymentDTO
{
    public class VNPayPaymentRequestDTO
    {
        public decimal amount { get; set; } 
        public string OrderDescription { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
    }
}
