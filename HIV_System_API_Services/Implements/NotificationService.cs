using HIV_System_API_BOs;
using HIV_System_API_DTOs.AppointmentDTO;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
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

        public NotificationService()
        {
            _notificationRepo = new NotificationRepo();
        }

        public async Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationRequestDTO notificationDto)
        {
            var notification = new Notification
            {
                NotiType = notificationDto.NotiType,
                NotiMessage = notificationDto.NotiMessage,
                SendAt = notificationDto.SendAt
            };

            var created = await _notificationRepo.CreateNotificationAsync(notification);
            return MapToResponseDTO(created);
        }

        public Task<bool> DeleteNotificationByIdAsync(int id)
        {
            return _notificationRepo.DeleteNotificationByIdAsync(id);
        }

        public async Task<List<NotificationResponseDTO>> GetAllNotifications()
        {
            var notifications = await _notificationRepo.GetAllNotification();
            return notifications.Select(MapToResponseDTO).ToList();
        }

        public async Task<NotificationResponseDTO> GetNotificationByIdAsync(int id)
        {
            var notification = await _notificationRepo.GetNotificationByIdAsync(id);
            return MapToResponseDTO(notification);
        }

        public async Task<NotificationDetailResponseDTO> GetNotificationDetailsByIdAsync(int id)
        {
            var notification = await _notificationRepo.GetNotificationByIdAsync(id);
            var recipients = await _notificationRepo.GetNotificationRecipientsAsync(id);

            var detailsDto = new NotificationDetailResponseDTO
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

            return detailsDto;
        }

        public async Task<List<NotificationResponseDTO>> GetNotificationsByRecipientAsync(int accId)
        {
            var notifications = await _notificationRepo.GetNotificationsByRecipientAsync(accId);
            return notifications.Select(MapToResponseDTO).ToList();
        }

        public async Task<NotificationDetailResponseDTO> SendNotificationToAccIdAsync(int ntfId, int accId)
        {
            var notification = await _notificationRepo.SendNotificationToAccIdAsync(ntfId, accId);
            return await GetNotificationDetailsByIdAsync(notification.NtfId);
        }

        public async Task<NotificationDetailResponseDTO> SendNotificationToRoleAsync(int ntfId, byte role)
        {
            var notification = await _notificationRepo.SendNotificationToRoleAsync(ntfId, role);
            return await GetNotificationDetailsByIdAsync(notification.NtfId);
        }

        public async Task<bool> UpdateNotificationByIdAsync(int id, UpdateNotificationRequestDTO notificationDto)
        {
            var notification = new Notification
            {
                NtfId = id,
                NotiType = notificationDto.NotiType,
                NotiMessage = notificationDto.NotiMessage,
                SendAt = notificationDto.SendAt
            };

            return await _notificationRepo.UpdateNotificationByIdAsync(notification);
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

        public async Task<List<NotificationResponseDTO>> GetAllPersonalNotificationsAsync(int accId)
        {
            if (accId <= 0)
            {
                throw new ArgumentException("Account ID must be greater than zero.", nameof(accId));
            }

            var notifications = await _notificationRepo.GetAllPersonalNotificationsAsync(accId);
            if (notifications == null)
            {
                return new List<NotificationResponseDTO>();
            }

            return notifications.Select(MapToResponseDTO).ToList();
        }
    }
}
