using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class PaymentDAO : IPaymentDAO
    {
        private readonly HivSystemApiContext _context;
        private static PaymentDAO? _instance;

        public PaymentDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static PaymentDAO Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new PaymentDAO();
                }
                return _instance;
            }
        }

        public async Task<List<Payment>> GetAllPaymentAsync()
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Pmr)
                    .Include(p => p.Srv)
                    .ToListAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting all payments: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Pmr)
                    .Include(p => p.Srv)
                    .FirstOrDefaultAsync(p => p.PayId == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting payment by ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Payment>> GetPaymentsByPmrIdAsync(int pmrId)
        {
            try
            {
                return await _context.Payments
                    .Where(p => p.PmrId == pmrId)
                    .Include(p => p.Pmr)
                    .Include(p => p.Srv)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting payments by PMR ID {pmrId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            try
            {
                payment.CreatedAt = DateTime.UtcNow;
                payment .UpdatedAt = DateTime.UtcNow;
                payment.PaymentDate = DateTime.UtcNow;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return payment;
            }
            catch
            {
                Debug.WriteLine($"Error creating payment: {payment}");
                throw;
            }
        }

        public async Task<Payment> UpdatePaymentAsync(int payId, Payment payment)
        {
            try
            {
                var existingPayment = await _context.Payments.FindAsync(payId)
                    ?? throw new InvalidOperationException($"Payment with ID {payId} not found.");

                existingPayment.Amount = payment.Amount;
                existingPayment.Currency = payment.Currency;
                existingPayment.PaymentStatus = payment.PaymentStatus;
                existingPayment.PaymentMethod = payment.PaymentMethod;
                existingPayment.Notes = payment.Notes;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existingPayment;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating payment: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeletePaymentAsync(int payId)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(payId);
                if (payment == null) return false;

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting payment: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> UpdatePaymentStatusAsync(int payId, byte status)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(payId)
                    ?? throw new InvalidOperationException($"Payment with ID {payId} not found.");

                payment.PaymentStatus = status;
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return payment;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating payment status: {ex.Message}");
                throw;
            }
        }
    }
}
