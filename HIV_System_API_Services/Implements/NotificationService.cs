using HIV_System_API_BOs;
using HIV_System_API_DTOs.AppointmentDTO;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepo _notificationRepo;
        private readonly HivSystemApiContext _context;

        public NotificationService()
        {
            _notificationRepo = new NotificationRepo();
            _context = new HivSystemApiContext();
        }

        // Constructor for dependency injection
        public NotificationService(INotificationRepo notificationRepo, HivSystemApiContext context)
        {
            _notificationRepo = notificationRepo ?? throw new ArgumentNullException(nameof(notificationRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private static Notification MapCreateRequestToEntity(CreateNotificationRequestDTO requestDTO)
        {
            return new Notification
            {
                NotiType = requestDTO.NotiType,
                NotiMessage = requestDTO.NotiMessage,
                SendAt = requestDTO.SendAt
            };
        }

        private static Notification MapUpdateRequestToEntity(int ntfId, UpdateNotificationRequestDTO requestDTO)
        {
            return new Notification
            {
                NtfId = ntfId,
                NotiType = requestDTO.NotiType,
                NotiMessage = requestDTO.NotiMessage,
            };
        }

        private static NotificationResponseDTO MapToResponseDTO(Notification notification)
        {
            return new NotificationResponseDTO
            {
                NotiId = notification.NtfId,
                NotiType = notification.NotiType,
                NotiMessage = notification.NotiMessage,
                SendAt = notification.SendAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static NotificationDetailResponseDTO MapToDetailResponseDTO(Notification notification, List<NotificationAccount> recipients)
        {
            return new NotificationDetailResponseDTO
            {
                NotiId = notification.NtfId,
                NotiType = notification.NotiType,
                NotiMessage = notification.NotiMessage,
                SendAt = notification.SendAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Recipients = recipients.Select(r => new NotificationRecipientDTO
                {
                    AccId = r.AccId,
                    Fullname = r.Acc?.Fullname,
                    Role = r.Acc?.Roles ?? 0
                }).ToList()
            };
        }

        private static List<NotificationResponseDTO> MapToResponseDTOList(List<Notification> notifications)
        {
            return notifications.Select(MapToResponseDTO).ToList();
        }

        private static void ValidateId(int id, string paramName)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
            }
        }

        private static void ValidateCreateNotificationRequestDTO(CreateNotificationRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                throw new ArgumentNullException(nameof(requestDTO));
            }

            if (string.IsNullOrWhiteSpace(requestDTO.NotiType))
            {
                throw new ArgumentException("Notification type is required.", nameof(requestDTO.NotiType));
            }

            // New validation for NotiType length
            if (requestDTO.NotiType.Length > 20)
            {
                throw new ArgumentException("Notification type cannot exceed 20 characters.", nameof(requestDTO.NotiType));
            }

            if (string.IsNullOrWhiteSpace(requestDTO.NotiMessage))
            {
                throw new ArgumentException("Notification message is required.", nameof(requestDTO.NotiMessage));
            }

            // New validation for NotiMessage length
            if (requestDTO.NotiMessage.Length > 300)
            {
                throw new ArgumentException("Notification type cannot exceed 300 characters.", nameof(requestDTO.NotiMessage));
            }

            if (requestDTO.SendAt == default(DateTime))
            {
                throw new ArgumentException("Send date is required.", nameof(requestDTO.SendAt));
            }
        }

        private static void ValidateUpdateNotificationRequestDTO(UpdateNotificationRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                throw new ArgumentNullException(nameof(requestDTO));
            }

            if (string.IsNullOrWhiteSpace(requestDTO.NotiType))
            {
                throw new ArgumentException("Notification type is required.", nameof(requestDTO.NotiType));
            }

            if (string.IsNullOrWhiteSpace(requestDTO.NotiMessage))
            {
                throw new ArgumentException("Notification message is required.", nameof(requestDTO.NotiMessage));
            }
        }

        private async Task ValidateAccountExists(int accId)
        {
            if (accId <= 0)
            {
                throw new ArgumentException("Account ID must be greater than 0.", nameof(accId));
            }

            var exists = await _context.Accounts.AnyAsync(a => a.AccId == accId);
            if (!exists)
            {
                throw new InvalidOperationException($"Account with ID {accId} does not exist.");
            }
        }

        private async Task ValidateNotificationExists(int ntfId)
        {
            if (ntfId <= 0)
            {
                throw new ArgumentException("Notification ID must be greater than 0.", nameof(ntfId));
            }

            var exists = await _context.Notifications.AnyAsync(n => n.NtfId == ntfId);
            if (!exists)
            {
                throw new InvalidOperationException($"Notification with ID {ntfId} does not exist.");
            }
        }

        private static void ValidateRole(byte role)
        {
            // Assuming roles are defined as: 0 = Admin, 1 = Doctor, 2 = Patient, etc.
            // Adjust validation based on your role system
            if (role < 0 || role > 10) // Adjust upper bound as needed
            {
                throw new ArgumentException("Invalid role specified.", nameof(role));
            }
        }

        public async Task<List<NotificationResponseDTO>> GetAllNotificationsAsync()
        {
            try
            {
                var notifications = await _notificationRepo.GetAllNotificationsAsync();
                return MapToResponseDTOList(notifications);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving all notifications: {ex.InnerException}");
            }
        }

        public async Task<NotificationResponseDTO?> GetNotificationByIdAsync(int id)
        {
            try
            {
                ValidateId(id, nameof(id));

                var notification = await _notificationRepo.GetNotificationByIdAsync(id);
                return notification == null ? null : MapToResponseDTO(notification);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notification: {ex.InnerException}");
            }
        }

        public async Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationRequestDTO notificationDto)
        {
            try
            {
                ValidateCreateNotificationRequestDTO(notificationDto);

                var notification = MapCreateRequestToEntity(notificationDto);
                var created = await _notificationRepo.CreateNotificationAsync(notification);

                return MapToResponseDTO(created);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while creating notification: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error creating notification: {ex.InnerException}");
            }
        }

        public async Task<bool> UpdateNotificationByIdAsync(int id, UpdateNotificationRequestDTO notificationDto)
        {
            try
            {
                ValidateId(id, nameof(id));
                ValidateUpdateNotificationRequestDTO(notificationDto);

                await ValidateNotificationExists(id);

                var notification = MapUpdateRequestToEntity(id, notificationDto);
                return await _notificationRepo.UpdateNotificationByIdAsync(notification);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while updating notification: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating notification: {ex.InnerException}");
            }
        }

        public async Task<bool> DeleteNotificationByIdAsync(int id)
        {
            try
            {
                ValidateId(id, nameof(id));

                // Check if the notification exists
                await ValidateNotificationExists(id);

                return await _notificationRepo.DeleteNotificationByIdAsync(id);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deleting notification: {ex.InnerException}");
            }
        }

        public async Task<NotificationResponseDTO> SendNotificationToRoleAsync(int ntfId, byte role)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                ValidateRole(role);

                await ValidateNotificationExists(ntfId);

                var notification = await _notificationRepo.SendNotificationToRoleAsync(ntfId, role);
                return MapToResponseDTO(notification);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error sending notification to role: {ex.InnerException}");
            }
        }

        public async Task<NotificationResponseDTO> SendNotificationToAccIdAsync(int ntfId, int accId)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                ValidateId(accId, nameof(accId));

                await ValidateNotificationExists(ntfId);
                await ValidateAccountExists(accId);

                var notification = await _notificationRepo.SendNotificationToAccIdAsync(ntfId, accId);
                return MapToResponseDTO(notification);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error sending notification to account: {ex.InnerException}");
            }
        }

        public async Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                await ValidateNotificationExists(ntfId);

                return await _notificationRepo.GetNotificationRecipientsAsync(ntfId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notification recipients: {ex.InnerException}");
            }
        }

        public async Task<List<NotificationResponseDTO>> GetNotificationsByRecipientAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var notifications = await _notificationRepo.GetNotificationsByRecipientAsync(accId);
                return MapToResponseDTOList(notifications);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notifications for recipient: {ex.InnerException}");
            }
        }

        public async Task<List<NotificationResponseDTO>> GetAllPersonalNotificationsAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var notifications = await _notificationRepo.GetAllPersonalNotificationsAsync(accId);
                return notifications == null ? new List<NotificationResponseDTO>() : MapToResponseDTOList(notifications);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving personal notifications: {ex.InnerException}");
            }
        }

        public async Task<List<NotificationResponseDTO>> GetAllUnreadNotificationsAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var notifications = await _notificationRepo.GetAllUnreadNotificationsAsync(accId);
                return notifications == null ? new List<NotificationResponseDTO>() : MapToResponseDTOList(notifications);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving unread notifications: {ex.InnerException}");
            }
        }

        public async Task<NotificationResponseDTO> ViewNotificationAsync(int ntfId, int accId)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                ValidateId(accId, nameof(accId));

                await ValidateNotificationExists(ntfId);
                await ValidateAccountExists(accId);

                var result = await _notificationRepo.ViewNotificationAsync(ntfId, accId);
                if (result == null)
                {
                    throw new InvalidOperationException($"Notification with ID {ntfId} could not be marked as viewed for account {accId}.");
                }

                return MapToResponseDTO(result);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error viewing notification: {ex.InnerException}");
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int ntfId, int accId)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                ValidateId(accId, nameof(accId));

                await ValidateNotificationExists(ntfId);
                await ValidateAccountExists(accId);

                return await _notificationRepo.MarkNotificationAsReadAsync(ntfId, accId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error marking notification as read: {ex.InnerException}");
            }
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                return await _notificationRepo.MarkAllNotificationsAsReadAsync(accId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error marking all notifications as read: {ex.InnerException}");
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                return await _notificationRepo.GetUnreadNotificationCountAsync(accId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving unread notification count: {ex.InnerException}");
            }
        }

        public async Task<List<NotificationAccount>> GetPersonalNotificationAccountsAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                return await _notificationRepo.GetPersonalNotificationAccountsAsync(accId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving personal notification accounts: {ex.InnerException}");
            }
        }

        public async Task<bool> DeleteNotificationForAccountAsync(int ntfId, int accId)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                ValidateId(accId, nameof(accId));

                await ValidateNotificationExists(ntfId);
                await ValidateAccountExists(accId);

                return await _notificationRepo.DeleteNotificationForAccountAsync(ntfId, accId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting notification for account: {ex.InnerException}");
            }
        }

        public async Task<NotificationDetailResponseDTO?> GetNotificationDetailsByIdAsync(int ntfId)
        {
            try
            {
                ValidateId(ntfId, nameof(ntfId));
                await ValidateNotificationExists(ntfId);

                var notification = await _notificationRepo.GetNotificationByIdAsync(ntfId);
                if (notification == null)
                {
                    return null;
                }

                var recipients = await _notificationRepo.GetNotificationRecipientsAsync(ntfId);
                return MapToDetailResponseDTO(notification, recipients);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notification details: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<NotificationResponseDTO> CreateAndSendToAccountIdAsync(CreateNotificationRequestDTO dto, int accId)
        {
            ValidateCreateNotificationRequestDTO(dto);
            await ValidateAccountExists(accId);

            var notification = MapCreateRequestToEntity(dto);
            var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

            await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, accId);

            return MapToResponseDTO(createdNotification);
        }

        public async Task<NotificationResponseDTO> CreateAndSendToRoleAsync(CreateNotificationRequestDTO dto, byte role)
        {
            ValidateCreateNotificationRequestDTO(dto);
            ValidateRole(role);

            var notification = MapCreateRequestToEntity(dto);
            var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

            await _notificationRepo.SendNotificationToRoleAsync(createdNotification.NtfId, role);

            return MapToResponseDTO(createdNotification);
        }

        public async Task<NotificationResponseDTO> CreateAndSendToAllAsync(CreateNotificationRequestDTO dto)
        {
            ValidateCreateNotificationRequestDTO(dto);

            var notification = MapCreateRequestToEntity(dto);
            var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

            var allAccountIds = await _context.Accounts.Select(a => a.AccId).ToListAsync();

            foreach (var accId in allAccountIds)
            {
                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, accId);
            }

            return MapToResponseDTO(createdNotification);
        }

    }
}