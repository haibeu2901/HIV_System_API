using HIV_System_API_Backend.Common;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("GetAllNotifications")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetAllNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetAllNotifications();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetNotificationById/{id:int}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<ActionResult<NotificationResponseDTO>> GetNotificationByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound($"Notification with ID {id} not found.");
                }
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetNotificationDetails/{id:int}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<ActionResult<NotificationDetailResponseDTO>> GetNotificationDetailsAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }
            try
            {
                var notification = await _notificationService.GetNotificationDetailsByIdAsync(id);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("CreateNotification")]
        [Authorize(Roles = "1,4,5")]
        public async Task<ActionResult<NotificationResponseDTO>> CreateNotificationAsync([FromBody] CreateNotificationRequestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Notification data is required.");
            }
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(dto);
                var createdNotification = await _notificationService.GetNotificationByIdAsync(notification.NotiId);
                return Created($"/api/Notification/GetNotificationById/{notification.NotiId}", createdNotification);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPut("UpdateNotibyId/{id:int}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<IActionResult> UpdateNotificationByIdAsync(int id, [FromBody] UpdateNotificationRequestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid notification data.");
            }
            try
            {
                var result = await _notificationService.UpdateNotificationByIdAsync(id, dto);
                if (!result)
                {
                    return NotFound($"Notification with ID {id} not found.");
                }

                var updatedNotification = await _notificationService.GetNotificationByIdAsync(id);
                return Ok(updatedNotification);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpDelete("DeleteNotibyId/{id:int}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<IActionResult> DeleteNotificationByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }
            var deleted = await _notificationService.DeleteNotificationByIdAsync(id);
            if (!deleted)
            {
                return NotFound($"Notification with ID {id} not found.");
            }
            return NoContent();
        }

        [HttpGet("GetByRecipient/{accId:int}")]
        [Authorize]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetNotificationsByRecipientAsync(int accId)
        {
            if (accId <= 0)
            {
                return BadRequest("Invalid account ID.");
            }
            try
            {
                var notifications = await _notificationService.GetNotificationsByRecipientAsync(accId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("sendToRole/{ntfId:int}/{role}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<ActionResult<NotificationDetailResponseDTO>> SendNotificationToRoleAsync(int ntfId, byte role)
        {
            if (ntfId <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }
            try
            {
                var notification = await _notificationService.SendNotificationToRoleAsync(ntfId, role);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("sendToAccId/{ntfId:int}/{accId:int}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<ActionResult<NotificationDetailResponseDTO>> SendNotificationToAccIdAsync(int ntfId, int accId)
        {
            if (ntfId <= 0 || accId <= 0)
            {
                return BadRequest("Invalid notification ID or account ID.");
            }
            try
            {
                var notification = await _notificationService.SendNotificationToAccIdAsync(ntfId, accId);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("GetAllPersonalNotifications")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetAllPersonalNotificationsAsync()
        {
            int accId = ClaimsHelper.ExtractAccountIdFromClaims(User) ?? 0;

            if (accId <= 0)
            {
                return BadRequest("Invalid account ID.");
            }
            try
            {
                var notifications = await _notificationService.GetAllPersonalNotificationsAsync(accId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetAllUnreadNotifications")]
        [Authorize]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetAllUnreadNotificationsAsync()
        {
            int accId = ClaimsHelper.ExtractAccountIdFromClaims(User) ?? 0;
            if (accId <= 0)
            {
                return BadRequest("Invalid account ID.");
            }
            try
            {
                var notifications = await _notificationService.GetAllUnreadNotificationsAsync(accId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ViewNotification/{ntfId}")]
        [Authorize]
        public async Task<ActionResult<Notification>> ViewNotificationAsync(int ntfId)
        {
            int accId = ClaimsHelper.ExtractAccountIdFromClaims(User) ?? 0;
            if (ntfId <= 0 || accId <= 0)
            {
                return BadRequest("Invalid notification ID or account ID.");
            }
            try
            {
                var notification = await _notificationService.ViewNotificationAsync(ntfId, accId);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}
