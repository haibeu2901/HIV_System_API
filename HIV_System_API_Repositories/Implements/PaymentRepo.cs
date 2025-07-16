using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class PaymentRepo : IPaymentRepo
    {
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await PaymentDAO.Instance.GetAllPaymentAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int payId)
        {
            return await PaymentDAO.Instance.GetPaymentByIdAsync(payId);
        }

        public async Task<List<Payment>> GetPaymentsByPatientMedicalRecordIdAsync(int pmrId)
        {
            return await PaymentDAO.Instance.GetPaymentsByPmrIdAsync(pmrId);
        }

        public async Task<List<Payment>> GetPersonalPaymentsAsync(int patientId)
        {
            return await PaymentDAO.Instance.GetPersonalPaymentsAsync(patientId);
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            return await PaymentDAO.Instance.CreatePaymentAsync(payment);
        }

        public async Task<Payment> UpdatePaymentAsync(int payId, Payment payment)
        {
            return await PaymentDAO.Instance.UpdatePaymentAsync(payId, payment);
        }

        public async Task<bool> DeletePaymentAsync(int payId)
        {
            return await PaymentDAO.Instance.DeletePaymentAsync(payId);
        }

        public async Task<Payment> UpdatePaymentStatusAsync(int payId, byte status)
        {
            return await PaymentDAO.Instance.UpdatePaymentStatusAsync(payId, status);
        }
    }
}
