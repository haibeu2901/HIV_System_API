using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IPaymentRepo
    {
        Task<List<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int payId);
        Task<List<Payment>> GetPaymentsByPatientMedicalRecordIdAsync(int pmrId);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(int payId, Payment payment);
        Task<bool> DeletePaymentAsync(int payId);
        Task<Payment> UpdatePaymentStatusAsync(int payId, byte status);
    }
}
