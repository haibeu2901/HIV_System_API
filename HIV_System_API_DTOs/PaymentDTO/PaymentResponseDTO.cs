using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PaymentDTO
{
    public class PaymentResponseDTO
    {
        public int PayId { get; set; }
        public int PmrId { get; set; }
        public int? SrvId { get; set; }
        public string? PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
        public byte PaymentStatus { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientEmail { get; set; }
        public string? ServiceName { get; set; }
        public double? ServicePrice { get; set; }
    }
}
