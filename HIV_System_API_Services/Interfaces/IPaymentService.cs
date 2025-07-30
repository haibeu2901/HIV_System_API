using HIV_System_API_DTOs.PaymentDTO;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(string ClientSecret, PaymentResponseDTO Payment)> CreatePaymentWithIntentAsync(PaymentRequestDTO dto);
        Task<PaymentResponseDTO?> GetPaymentByIntentIdAsync(string paymentIntentId);
        Task UpdatePaymentStatusByIntentIdAsync(string paymentIntentId, byte status);
        Task<PaymentResponseDTO> UpdatePaymentStatusByIdAsync(int paymentId, byte status); 
        Task<List<PaymentResponseDTO>> GetAllPaymentsAsync();
        Task<PaymentResponseDTO> GetPaymentByIdAsync(int id);
        Task<List<PaymentResponseDTO>> GetPaymentsByPmrIdAsync(int pmrId);
        Task<List<PaymentResponseDTO>> GetAllPersonalPaymentsAsync(int accId);
        Task<PaymentResponseDTO> UpdatePaymentAsync(int payId, PaymentRequestDTO dto);
        Task<bool> DeletePaymentAsync(int payId);
    }
}
