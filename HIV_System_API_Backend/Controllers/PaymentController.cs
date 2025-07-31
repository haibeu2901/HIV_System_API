using HIV_System_API_Backend.Common;
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

        // Test card numbers for different scenarios
        private readonly Dictionary<string, (string PaymentMethod, string Description, bool IsSuccess)> _testCardNumbers = new()
        {
            // Success scenarios
            { "4242424242424242", ("pm_card_visa", "Visa - Success", true) },
            { "4000056655665556", ("pm_card_visa_debit", "Visa Debit - Success", true) },
            { "5555555555554444", ("pm_card_mastercard", "Mastercard - Success", true) },
            { "2223003122003222", ("pm_card_mastercard", "Mastercard 2-series - Success", true) },
            { "378282246310005", ("pm_card_amex", "American Express - Success", true) },
            { "6011111111111117", ("pm_card_discover", "Discover - Success", true) },
            { "3056930009020004", ("pm_card_diners", "Diners Club - Success", true) },
            { "30569309025904", ("pm_card_diners", "Diners Club 14-digit - Success", true) },
            { "6200000000000005", ("pm_card_unionpay", "UnionPay - Success", true) },
            { "3566002020360505", ("pm_card_jcb", "JCB - Success", true) },

            // Failure scenarios
            { "4000000000000002", ("pm_card_chargeDeclined", "Visa - Generic decline", false) },
            { "4000000000009995", ("pm_card_chargeDeclinedInsufficientFunds", "Visa - Insufficient funds", false) },
            { "4000000000009987", ("pm_card_chargeDeclinedLostCard", "Visa - Lost card", false) },
            { "4000000000009979", ("pm_card_chargeDeclinedStolenCard", "Visa - Stolen card", false) },
            { "4000000000000069", ("pm_card_chargeDeclinedExpiredCard", "Visa - Expired card", false) },
            { "4000000000000127", ("pm_card_chargeDeclinedIncorrectCvc", "Visa - Incorrect CVC", false) },
            { "4000000000000119", ("pm_card_chargeDeclinedProcessingError", "Visa - Processing error", false) },
            { "4242424242424241", ("pm_card_chargeDeclinedIncorrectNumber", "Visa - Incorrect number", false) }
        };

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpPost("CreatePayment")]
        [Authorize(Roles = "1, 2, 4, 5")]
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
                        byte newStatus = stripeEvent.Type == "payment_intent.succeeded" ? (byte)2 : (byte)3; // 2: Success, 3: Failed
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
        public async Task<IActionResult> ConfirmPayment(string id, [FromBody] TestCardRequestDTO request)
        {
            try
            {
                // Remove spaces and validate card number format
                var cardNumber = request.CardNumber?.Replace(" ", "");

                if (string.IsNullOrEmpty(cardNumber))
                {
                    return BadRequest(new { Error = "Card number is required" });
                }

                // Check if the card number exists in our test scenarios
                if (!_testCardNumbers.TryGetValue(cardNumber, out var cardInfo))
                {
                    return BadRequest(new
                    {
                        Error = "Invalid test card number",
                        Message = "Please use a valid test card number from the supported scenarios"
                    });
                }

                var service = new PaymentIntentService();
                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = cardInfo.PaymentMethod
                };

                var paymentIntent = await service.ConfirmAsync(id, options);

                // Update payment status based on card scenario
                byte newStatus = cardInfo.IsSuccess ? (byte)2 : (byte)3; // 2: Success, 3: Failed
                await _paymentService.UpdatePaymentStatusByIntentIdAsync(paymentIntent.Id, newStatus);

                return Ok(new
                {
                    Status = cardInfo.IsSuccess ? "succeeded" : "failed",
                    PaymentIntentId = paymentIntent.Id,
                    CustomerEmail = paymentIntent.Customer?.Email ?? paymentIntent.ReceiptEmail,
                    CardInfo = cardInfo.Description,
                    TestScenario = cardInfo.IsSuccess ? "Success" : "Failure",
                    Message = cardInfo.IsSuccess
        ? "Thanh toán thành công! Thông báo đã được gửi."
        : "Thanh toán thất bại! Thông báo đã được gửi."
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("fail-payment/{id}")]
        [Authorize]
        public async Task<IActionResult> FailPayment(string id, [FromBody] TestCardRequestDTO request)
        {
            try
            {
                // Remove spaces and validate card number format
                var cardNumber = request.CardNumber?.Replace(" ", "");

                if (string.IsNullOrEmpty(cardNumber))
                {
                    return BadRequest(new { Error = "Card number is required" });
                }

                // Check if the card number exists in our test scenarios
                if (!_testCardNumbers.TryGetValue(cardNumber, out var cardInfo))
                {
                    return BadRequest(new
                    {
                        Error = "Invalid test card number",
                        Message = "Please use a valid test card number from the supported scenarios"
                    });
                }

                var service = new PaymentIntentService();
                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = cardInfo.PaymentMethod
                };

                var paymentIntent = await service.ConfirmAsync(id, options);

                // Always set to failed status for this endpoint, regardless of card scenario
                byte newStatus = (byte)3; // 3: Failed
                await _paymentService.UpdatePaymentStatusByIntentIdAsync(paymentIntent.Id, newStatus);

                return Ok(new
                {
                    Status = "failed",
                    PaymentIntentId = paymentIntent.Id,
                    CustomerEmail = paymentIntent.Customer?.Email ?? paymentIntent.ReceiptEmail,
                    CardInfo = cardInfo.Description,
                    TestScenario = "Forced Failure"
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("test-card-numbers")]
        [Authorize]
        public IActionResult GetTestCardNumbers()
        {
            var successCards = _testCardNumbers.Where(x => x.Value.IsSuccess)
                .Select(x => new { CardNumber = x.Key, Description = x.Value.Description })
                .ToList();

            var failureCards = _testCardNumbers.Where(x => !x.Value.IsSuccess)
                .Select(x => new { CardNumber = x.Key, Description = x.Value.Description })
                .ToList();

            return Ok(new
            {
                SuccessScenarios = successCards,
                FailureScenarios = failureCards
            });
        }

        [HttpGet("GetAllPersonalPayments")]
        [Authorize(Roles = "3")]
        public async Task<ActionResult<List<PaymentResponseDTO>>> GetAllPersonalPayments()
        {
            try
            {
                var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (accId == null)
                {
                    return Unauthorized("Account ID not found in token.");
                }

                var result = await _paymentService.GetAllPersonalPaymentsAsync(accId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Manually update payment status (for cash payments by doctors/staff)
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated payment information</returns>
        /// <response code="200">Returns the updated payment</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission</response>
        /// <response code="404">If the payment was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPatch("UpdatePaymentStatus/{id}")]
        [Authorize(Roles = "1, 2, 4, 5")] // Admin, Doctor, Staff, Manager
        public async Task<ActionResult<PaymentResponseDTO>> UpdatePaymentStatus(int id, [FromBody] PaymentStatusUpdateRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { Error = "Request body is required" });
                }

                // Validate status (1=Pending, 2=Success, 3=Failed)
                if (request.Status < 1 || request.Status > 3)
                {
                    return BadRequest(new { Error = "Invalid payment status. Status must be 1 (Pending), 2 (Success), or 3 (Failed)" });
                }

                var result = await _paymentService.UpdatePaymentStatusByIdAsync(id, request.Status);

                var statusText = request.Status switch
                {
                    1 => "pending",
                    2 => "success",
                    3 => "failed",
                    _ => "unknown"
                };

                return Ok(new
                {
                    Message = $"Payment status updated to {statusText} successfully. Notification sent to patient.",
                    Payment = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = $"Payment with ID {id} not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Mark payment as successful for cash payments (quick action for doctors/staff)
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Updated payment information</returns>
        /// <response code="200">Returns the updated payment</response>
        /// <response code="404">If the payment was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("MarkCashPaymentSuccess/{id}")]
        [Authorize(Roles = "1, 2, 4, 5")] // Admin, Doctor, Staff, Manager
        public async Task<ActionResult<PaymentResponseDTO>> MarkCashPaymentSuccess(int id)
        {
            try
            {
                var result = await _paymentService.UpdatePaymentStatusByIdAsync(id, 2); // 2 = Success

                return Ok(new
                {
                    Message = "Cash payment marked as successful. Notification sent to patient.",
                    Payment = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = $"Payment with ID {id} not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}