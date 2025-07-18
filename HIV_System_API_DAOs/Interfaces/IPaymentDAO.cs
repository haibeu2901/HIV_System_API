using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IPaymentDAO
    {
        Task<List<Payment>> GetAllPaymentAsync();
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<List<Payment>> GetPaymentsByPmrIdAsync(int pmrId);
        Task<List<Payment>> GetPersonalPaymentsAsync(int patientId);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(int payId, Payment payment);
        Task<bool> DeletePaymentAsync(int payId);
        Task<Payment> UpdatePaymentStatusAsync(int payId, byte status);
    }
}
