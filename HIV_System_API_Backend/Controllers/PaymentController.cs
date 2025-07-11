using HIV_System_API_DTOs.PaymentDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpPost("CreatePayment")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                var (clientSecret, payment) = await _paymentService.CreatePaymentWithIntentAsync(request);
                return Ok(new { ClientSecret = clientSecret, Payment = payment });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.InnerException });
            }
        }

        [HttpPost("StripeWebhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var webhookSecret = _configuration["Stripe:WebhookSecret"];
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                if (stripeEvent.Type == "payment_intent.succeeded" || stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        byte newStatus = stripeEvent.Type == "payment_intent.succeeded" ? (byte)1 : (byte)2; // 1: Success, 2: Failed
                        await _paymentService.UpdatePaymentStatusByIntentIdAsync(paymentIntent.Id, newStatus);
                    }
                }
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(new { Error = e.Message });
            }
        }

        [HttpGet("GetAllPayments")]
        [Authorize]
        public async Task<ActionResult<List<PaymentResponseDTO>>> GetAllPayments()
        {
            var result = await _paymentService.GetAllPaymentsAsync();
            return Ok(result);
        }

        [HttpGet("GetPaymentById/{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentResponseDTO>> GetPaymentById(int id)
        {
            try
            {
                var result = await _paymentService.GetPaymentByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("GetPaymentsByPmrId/{pmrId}")]
        [Authorize]
        public async Task<ActionResult<List<PaymentResponseDTO>>> GetPaymentsByPmrId(int pmrId)
        {
            var result = await _paymentService.GetPaymentsByPmrIdAsync(pmrId);
            return Ok(result);
        }

        [HttpPut("UpdatePayment/{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentResponseDTO>> UpdatePayment(int id, [FromBody] PaymentRequestDTO request)
        {
            try
            {
                var result = await _paymentService.UpdatePaymentAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("DeletePayment/{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var deleted = await _paymentService.DeletePaymentAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
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

        [HttpPost("fail-payment/{id}")]
        [Authorize]
        public async Task<IActionResult> FailPayment(string id)
        {
            try
            {
                var service = new PaymentIntentService();
                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = "pm_card_chargeDeclined" // Simulates Visa: 4000 0000 0000 0002
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
