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
        Task<PaymentIntent> CreatePaymentIntent(PaymentRequestDTO request);
    }
}
