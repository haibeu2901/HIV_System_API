using HIV_System_API_DTOs.PaymentDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-payment-intent")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequestDTO request)
        {
            try
            {
                var paymentIntent = await _paymentService.CreatePaymentIntent(request);
                return Ok(new { ClientSecret = paymentIntent.ClientSecret, PaymentIntentId = paymentIntent.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("payment-status/{id}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(string id)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(id);
                return Ok(new
                {
                    Status = paymentIntent.Status,
                    Amount = paymentIntent.Amount,
                    Currency = paymentIntent.Currency,
                    Description = paymentIntent.Description,
                    CustomerEmail = paymentIntent.Customer?.Email ?? paymentIntent.ReceiptEmail
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("confirm-payment/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmPayment(string id)
        {
            try
            {
                var service = new PaymentIntentService();
                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = "pm_card_visa" // Simulates Visa: 4242 4242 4242 4242
                };
                var paymentIntent = await service.ConfirmAsync(id, options);
                return Ok(new { Status = paymentIntent.Status, PaymentIntentId = paymentIntent.Id, CustomerEmail = paymentIntent.Customer?.Email ?? paymentIntent.ReceiptEmail });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
