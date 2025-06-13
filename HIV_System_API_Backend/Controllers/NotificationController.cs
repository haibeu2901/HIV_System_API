using HIV_System_API_BOs;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
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
        [HttpGet("GetAllNoti")]
        public async Task<IActionResult> GetAllNotification()
        {
            try
            {
                var notifications = await _notificationService.GetAllNotifications();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetNotificationById/{id:int}")]
        public async Task<ActionResult<NotificationDTO>> GetNotificationByIdAsync(int id)
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateNotification")]
        public async Task<ActionResult<NotificationDTO>> CreateNotificationAsync([FromBody] CreateNotificationDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Notification data is required.");
            }
            try
            {
                var notification = new Notification
                {
                    NotiType = dto.NotiType,
                    NotiMessage = dto.NotiMessage,
                    SendAt = dto.SendAt,
                };

                var createdNotification = await _notificationService.CreateNotificationAsync(notification);
                if (createdNotification == null || createdNotification.NtfId <= 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create notification with a valid ID.");
                }

                var result = await _notificationService.GetNotificationByIdAsync(createdNotification.NtfId);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve created notification.");
                }

                // Return Created response with the correct route
                return Created($"/api/Notification/GetNotificationById/{createdNotification.NtfId}", result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("UpdateNotibyId")]
        public async Task<IActionResult> UpdateNotificationByIdAsync(int id, [FromBody] UpdateNotificationDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest("Invalid notification ID or data.");
            }
            try
            {
                // Create Notification object with updated values
                var notification = new Notification
                {
                    NtfId = id,  // Important: Include the ID
                    NotiType = dto.NotiType,
                    NotiMessage = dto.NotiMessage,
                    SendAt = dto.SendAt
                };

                var result = await _notificationService.UpdateNotificationByIdAsync(notification);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the notification.");
                }
                
                // Get updated notification to return
                var updatedNotification = await _notificationService.GetNotificationByIdAsync(id);
                return Ok(updatedNotification);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("DeleteNotibyId")]
        public async Task<IActionResult> DeleteNotificationByIdAsync(int id)
        {
            var deleted = await _notificationService.DeleteNotificationByIdAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpGet("SendtoAccId")]
        public async Task<IActionResult> GetNotificationByAccId(int accId)
        {
            var notification = await _notificationService.GetNotificationByAccId(accId);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }
        [HttpPost("sendByRole")]
        public async Task<IActionResult> SendNotificationByRoleAsync(int ntfId, byte role)
        {
            var notification = await _notificationService.SendNotificationByRoleAsync(ntfId, role);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }

        [HttpPost("sendByAccId/{ntfId}/{accId}")]
        public async Task<IActionResult> SendNotificationByAccIdAsync(int ntfId, int accId)
        {
            var notification = await _notificationService.SendNotificationByAccIdAsync(ntfId, accId);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);


        }
    }
}
