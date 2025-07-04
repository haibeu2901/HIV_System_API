using HIV_System_API_DTOs.PaymentDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaymentIntent> CreatePaymentIntent(PaymentRequestDTO request)
        {
            // Get logged-in user's email from claims
            var user = _httpContextAccessor.HttpContext.User;
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value ?? "default@example.com";

            // Create or retrieve customer in Stripe
            string customerId = null;
            if (!string.IsNullOrEmpty(email))
            {
                var customerService = new CustomerService();
                var customer = await customerService.ListAsync(new CustomerListOptions { Email = email });
                if (customer.Data.Any())
                {
                    customerId = customer.Data.First().Id; // Reuse existing customer
                }
                else
                {
                    var customerOptions = new CustomerCreateOptions
                    {
                        Email = email,
                        Description = "Customer for HIV System payment"
                    };
                    var newCustomer = await customerService.CreateAsync(customerOptions);
                    customerId = newCustomer.Id;
                }
            }

            long amountInSmallestUnit = (long)request.Amount; // Default: no scaling for VND
            if (request.Currency.ToLower() == "usd") // Scale for USD (cents)
            {
                amountInSmallestUnit = (long)(request.Amount * 100);
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInSmallestUnit, // Use correct amount
                Currency = request.Currency,
                Description = request.Description,
                PaymentMethodTypes = new List<string> { "card" },
                Customer = customerId,
                ReceiptEmail = email
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }
    }
}
