using HIV_System_API_DTOs.VNPayPaymentDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;

        public VNPayController(IConfiguration config)
        {
           _vnPayService = new VNPayService(config);
        }

        [HttpPost("CreatePaymentUrl")]
        [Authorize]
        public ActionResult<VNPayPaymentResponseDTO> CreatePayment([FromBody] VNPayPaymentRequestDTO request)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnPayService.CreatePaymentUrl(request, clientIp);
            return Ok(new VNPayPaymentResponseDTO { PaymentUrl = paymentUrl });
        }

        [HttpGet("PaymentReturn")]
        [AllowAnonymous]
        public IActionResult PaymentReturn()
        {
            var isValid = _vnPayService.ValidateSignature(Request.Query);
            if (isValid)
            {
                // Handle success (update order, etc.)
                return Ok("Payment success!");
            }
            else
            {
                // Handle failure
                return BadRequest("Invalid signature.");
            }
        }
    }
}
