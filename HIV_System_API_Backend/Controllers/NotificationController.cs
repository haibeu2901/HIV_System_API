using HIV_System_API_Backend.Common;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Get all notifications (Admin and specific roles only)
        /// </summary>
        [HttpGet("GetAllNotifications")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetAllNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetAllNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving notifications", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("GetNotificationById/{id:int}")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDTO>> GetNotificationById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);

                if (notification == null)
                {
                    return NotFound(new { message = $"Notification with ID {id} not found." });
                }

                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed notification information including recipients
        /// </summary>
        [HttpGet("GetNotificationDetails/{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationDetailResponseDTO>> GetNotificationDetails(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            try
            {
                var notification = await _notificationService.GetNotificationDetailsByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = $"Notification with ID {id} not found." });
                }
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving notification details", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        [HttpPost("CreateNotification")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDTO>> CreateNotification([FromBody] CreateNotificationRequestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Notification data is required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var notification = await _notificationService.CreateNotificationAsync(dto);

                return CreatedAtAction(
                    nameof(GetNotificationById),
                    new { id = notification.NotiId },
                    notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing notification
        /// </summary>
        [HttpPut("UpdateNotification/{id:int}")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDTO>> UpdateNotification(int id, [FromBody] UpdateNotificationRequestDTO dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            if (dto == null)
            {
                return BadRequest(new { message = "Notification data is required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _notificationService.UpdateNotificationByIdAsync(id, dto);

                if (!result)
                {
                    return NotFound(new { message = $"Notification with ID {id} not found." });
                }

                var updatedNotification = await _notificationService.GetNotificationByIdAsync(id);
                return Ok(updatedNotification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("DeleteNotification/{id:int}")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            try
            {
                var deleted = await _notificationService.DeleteNotificationByIdAsync(id);

                if (!deleted)
                {
                    return NotFound(new { message = $"Notification with ID {id} not found." });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notifications for a specific recipient
        /// </summary>
        [HttpGet("GetByRecipient/{accId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetNotificationsByRecipient(int accId)
        {
            if (accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID. ID must be greater than 0." });
            }

            try
            {
                var notifications = await _notificationService.GetNotificationsByRecipientAsync(accId);
                return Ok(notifications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving notifications", error = ex.Message });
            }
        }

        /// <summary>
        /// Send notification to users with specific role
        /// </summary>
        [HttpPost("SendToRole/{ntfId:int}/{role}")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDTO>> SendNotificationToRole(int ntfId, byte role)
        {
            if (ntfId <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            try
            {
                var notification = await _notificationService.SendNotificationToRoleAsync(ntfId, role);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while sending notification to role", error = ex.Message });
            }
        }

        /// <summary>
        /// Send notification to specific account
        /// </summary>
        [HttpPost("SendToAccount/{ntfId:int}/{accId:int}")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDTO>> SendNotificationToAccount(int ntfId, int accId)
        {
            if (ntfId <= 0 || accId <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID or account ID. IDs must be greater than 0." });
            }

            try
            {
                var notification = await _notificationService.SendNotificationToAccIdAsync(ntfId, accId);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while sending notification to account", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all personal notifications for the authenticated user
        /// </summary>
        [HttpGet("GetPersonalNotifications")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetPersonalNotifications()
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var notifications = await _notificationService.GetAllPersonalNotificationsAsync(accId.Value);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving personal notifications", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all unread notifications for the authenticated user
        /// </summary>
        [HttpGet("GetUnreadNotifications")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetUnreadNotifications()
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var notifications = await _notificationService.GetAllUnreadNotificationsAsync(accId.Value);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving unread notifications", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark a notification as viewed by the authenticated user
        /// </summary>
        [HttpPut("ViewNotification/{ntfId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDTO>> ViewNotification(int ntfId)
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (ntfId <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var notification = await _notificationService.ViewNotificationAsync(ntfId, accId.Value);
                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while viewing notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Get count of unread notifications for the authenticated user
        /// </summary>
        [HttpGet("GetUnreadCount")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetUnreadNotificationCount()
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(accId.Value);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving unread notification count", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark a notification as read by the authenticated user
        /// </summary>
        [HttpPut("MarkAsRead/{ntfId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> MarkNotificationAsRead(int ntfId)
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (ntfId <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(ntfId, accId.Value);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while marking notification as read", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read for the authenticated user
        /// </summary>
        [HttpPut("MarkAllAsRead")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> MarkAllNotificationsAsRead()
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(accId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while marking all notifications as read", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notification recipients for a specific notification
        /// </summary>
        [HttpGet("GetRecipients/{ntfId:int}")]
        [Authorize(Roles = "1,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<NotificationRecipientDTO>>> GetNotificationRecipients(int ntfId)
        {
            if (ntfId <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            try
            {
                var recipients = await _notificationService.GetNotificationRecipientsAsync(ntfId);

                var dtoList = recipients.Select(r => new NotificationRecipientDTO
                {
                    AccId = r.AccId,
                    Fullname = r.Acc?.Fullname,
                    Role = r.Acc?.Roles ?? 0,
                    IsRead = r.IsRead
                }).ToList();

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notification recipients.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification for a specific account
        /// </summary>
        [HttpDelete("DeleteForAccount/{ntfId:int}")]
        [Authorize(Roles = "1")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNotificationForAccount(int ntfId)
        {
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (ntfId <= 0)
            {
                return BadRequest(new { message = "Invalid notification ID. ID must be greater than 0." });
            }

            if (accId == null || accId <= 0)
            {
                return BadRequest(new { message = "Invalid account ID from token." });
            }

            try
            {
                var result = await _notificationService.DeleteNotificationForAccountAsync(ntfId, accId.Value);

                if (!result)
                {
                    return NotFound(new { message = $"Notification with ID {ntfId} not found for account {accId}." });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting notification for account", error = ex.Message });
            }
        }

        /// <summary>
        /// Create notification and send to specific account
        /// </summary>
        [HttpPost("CreateAndSendToAccount/{accId:int}")]
        [Authorize(Roles = "1,5")]
        public async Task<ActionResult<NotificationResponseDTO>> CreateAndSendToAccount(int accId, [FromBody] CreateNotificationRequestDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Notification data is required." });

            if (accId <= 0)
                return BadRequest(new { message = "Invalid account ID." });

            try
            {
                var notification = await _notificationService.CreateAndSendToAccountIdAsync(dto, accId);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notification.NotiId }, notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating and sending notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create notification and send to specific role
        /// </summary>
        [HttpPost("CreateAndSendToRole/{role}")]
        [Authorize(Roles = "1,5")]
        public async Task<ActionResult<NotificationResponseDTO>> CreateAndSendToRole(byte role, [FromBody] CreateNotificationRequestDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Notification data is required." });

            try
            {
                var notification = await _notificationService.CreateAndSendToRoleAsync(dto, role);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notification.NotiId }, notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating and sending notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create notification and send to all users
        /// </summary>
        [HttpPost("CreateAndSendToAll")]
        [Authorize(Roles = "1,5")]
        public async Task<ActionResult<NotificationResponseDTO>> CreateAndSendToAll([FromBody] CreateNotificationRequestDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Notification data is required." });

            try
            {
                var notification = await _notificationService.CreateAndSendToAllAsync(dto);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notification.NotiId }, notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating and sending notification to all.", error = ex.Message });
            }
        }

    }
}
