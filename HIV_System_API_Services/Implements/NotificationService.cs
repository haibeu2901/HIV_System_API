using HIV_System_API_BOs;
using HIV_System_API_DTOs.AppointmentDTO;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        public NotificationService()
        {
            _notificationRepo = new NotificationRepo();
            _context = new HivSystemApiContext();
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        // Constructor for dependency injection
        public NotificationService(INotificationRepo notificationRepo, HivSystemApiContext context, IMemoryCache cache)
        {
            _notificationRepo = notificationRepo ?? throw new ArgumentNullException(nameof(notificationRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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

            // Use cache for account existence check
            var cacheKey = $"account_exists_{accId}";
            if (!_cache.TryGetValue(cacheKey, out bool exists))
            {
                exists = await _context.Accounts.AnyAsync(a => a.AccId == accId);
                _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(5));
            }
            
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

            // Use cache for notification existence check
            var cacheKey = $"notification_exists_{ntfId}";
            if (!_cache.TryGetValue(cacheKey, out bool exists))
            {
                exists = await _context.Notifications.AnyAsync(n => n.NtfId == ntfId);
                _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(5));
            }

            if (!exists)
            {
                throw new InvalidOperationException($"Notification with ID {ntfId} does not exist.");
            }
        }

        private static void ValidateRole(byte role)
        {
            // Assuming roles are defined as: 1 = Admin, 2 = Doctor, 3 = Patient, 4 = Staff, 5 = Manager
            if (role < 1 || role > 5)
            {
                throw new ArgumentException("Invalid role specified.", nameof(role));
            }
        }

        public async Task<List<NotificationResponseDTO>> GetAllNotificationsAsync()
        {
            try
            {
                var cacheKey = "all_notifications";
                if (_cache.TryGetValue(cacheKey, out List<NotificationResponseDTO>? cachedNotifications) && cachedNotifications != null)
                {
                    return cachedNotifications;
                }

                var notifications = await _notificationRepo.GetAllNotificationsAsync();
                var result = MapToResponseDTOList(notifications);
                
                // Cache for 2 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
                
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving all notifications: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<NotificationResponseDTO?> GetNotificationByIdAsync(int id)
        {
            try
            {
                ValidateId(id, nameof(id));

                var cacheKey = $"notification_{id}";
                if (_cache.TryGetValue(cacheKey, out NotificationResponseDTO? cachedNotification) && cachedNotification != null)
                {
                    return cachedNotification;
                }

                var notification = await _notificationRepo.GetNotificationByIdAsync(id);
                if (notification == null) return null;
                
                var result = MapToResponseDTO(notification);
                
                // Cache for 5 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                
                return result;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notification: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationRequestDTO notificationDto)
        {
            try
            {
                ValidateCreateNotificationRequestDTO(notificationDto);

                var notification = MapCreateRequestToEntity(notificationDto);
                var created = await _notificationRepo.CreateNotificationAsync(notification);

                // Clear cache
                _cache.Remove("all_notifications");

                return MapToResponseDTO(created);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error creating notification: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error creating notification: {ex.InnerException?.Message ?? ex.Message}");
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
                var result = await _notificationRepo.UpdateNotificationByIdAsync(notification);
                
                // Clear cache
                _cache.Remove("all_notifications");
                _cache.Remove($"notification_{id}");
                
                return result;
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
                throw new InvalidOperationException($"Database error updating notification: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating notification: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<bool> DeleteNotificationByIdAsync(int id)
        {
            try
            {
                ValidateId(id, nameof(id));

                // Check if the notification exists
                await ValidateNotificationExists(id);

                var result = await _notificationRepo.DeleteNotificationByIdAsync(id);
                
                // Clear cache
                _cache.Remove("all_notifications");
                _cache.Remove($"notification_{id}");
                _cache.Remove($"notification_exists_{id}");
                
                return result;
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
                throw new InvalidOperationException($"Unexpected error deleting notification: {ex.InnerException?.Message ?? ex.Message}");
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
                
                // Clear related caches
                _cache.Remove("all_notifications");
                
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
                throw new InvalidOperationException($"Error sending notification to role: {ex.InnerException?.Message ?? ex.Message}");
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
                
                // Clear user-specific caches
                _cache.Remove($"personal_notifications_{accId}");
                _cache.Remove($"unread_notifications_{accId}");
                _cache.Remove($"unread_count_{accId}");
                
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
                throw new InvalidOperationException($"Error sending notification to account: {ex.InnerException?.Message ?? ex.Message}");
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
                throw new InvalidOperationException($"Error retrieving notification recipients: {ex.InnerException?.Message ?? ex.Message}");
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
                throw new InvalidOperationException($"Error retrieving notifications for recipient: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<List<NotificationResponseDTO>> GetAllPersonalNotificationsAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var cacheKey = $"personal_notifications_{accId}";
                if (_cache.TryGetValue(cacheKey, out List<NotificationResponseDTO>? cachedNotifications) && cachedNotifications != null)
                {
                    return cachedNotifications;
                }

                var notifications = await _notificationRepo.GetAllPersonalNotificationsAsync(accId);
                var result = notifications == null ? new List<NotificationResponseDTO>() : MapToResponseDTOList(notifications);
                
                // Cache for 3 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
                
                return result;
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
                throw new InvalidOperationException($"Error retrieving personal notifications: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<List<NotificationResponseDTO>> GetAllUnreadNotificationsAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var cacheKey = $"unread_notifications_{accId}";
                if (_cache.TryGetValue(cacheKey, out List<NotificationResponseDTO>? cachedNotifications) && cachedNotifications != null)
                {
                    return cachedNotifications;
                }

                var notifications = await _notificationRepo.GetAllUnreadNotificationsAsync(accId);
                var result = notifications == null ? new List<NotificationResponseDTO>() : MapToResponseDTOList(notifications);
                
                // Cache for 1 minute since this changes frequently
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));
                
                return result;
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
                throw new InvalidOperationException($"Error retrieving unread notifications: {ex.InnerException?.Message ?? ex.Message}");
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
                    throw new InvalidOperationException($"Notification with ID {ntfId} cannot be marked as viewed for account {accId}.");
                }

                // Clear user-specific caches
                _cache.Remove($"personal_notifications_{accId}");
                _cache.Remove($"unread_notifications_{accId}");
                _cache.Remove($"unread_count_{accId}");

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
                throw new InvalidOperationException($"Error viewing notification: {ex.InnerException?.Message ?? ex.Message}");
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

                var result = await _notificationRepo.MarkNotificationAsReadAsync(ntfId, accId);
                
                // Clear user-specific caches
                _cache.Remove($"personal_notifications_{accId}");
                _cache.Remove($"unread_notifications_{accId}");
                _cache.Remove($"unread_count_{accId}");
                
                return result;
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
                throw new InvalidOperationException($"Error marking notification as read: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var result = await _notificationRepo.MarkAllNotificationsAsReadAsync(accId);
                
                // Clear user-specific caches
                _cache.Remove($"personal_notifications_{accId}");
                _cache.Remove($"unread_notifications_{accId}");
                _cache.Remove($"unread_count_{accId}");
                
                return result;
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
                throw new InvalidOperationException($"Error marking all notifications as read: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(int accId)
        {
            try
            {
                ValidateId(accId, nameof(accId));
                await ValidateAccountExists(accId);

                var cacheKey = $"unread_count_{accId}";
                if (_cache.TryGetValue(cacheKey, out int cachedCount))
                {
                    return cachedCount;
                }

                var count = await _notificationRepo.GetUnreadNotificationCountAsync(accId);
                
                // Cache for 30 seconds since this changes frequently
                _cache.Set(cacheKey, count, TimeSpan.FromSeconds(30));
                
                return count;
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
                throw new InvalidOperationException($"Error retrieving unread notification count: {ex.InnerException?.Message ?? ex.Message}");
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
                throw new InvalidOperationException($"Error retrieving personal notification accounts: {ex.InnerException?.Message ?? ex.Message}");
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

                var result = await _notificationRepo.DeleteNotificationForAccountAsync(ntfId, accId);
                
                // Clear user-specific caches
                _cache.Remove($"personal_notifications_{accId}");
                _cache.Remove($"unread_notifications_{accId}");
                _cache.Remove($"unread_count_{accId}");
                
                return result;
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
                throw new InvalidOperationException($"Error deleting notification for account: {ex.InnerException?.Message ?? ex.Message}");
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

            // Clear caches
            _cache.Remove("all_notifications");
            _cache.Remove($"personal_notifications_{accId}");
            _cache.Remove($"unread_notifications_{accId}");
            _cache.Remove($"unread_count_{accId}");

            return MapToResponseDTO(createdNotification);
        }

        public async Task<NotificationResponseDTO> CreateAndSendToRoleAsync(CreateNotificationRequestDTO dto, byte role)
        {
            ValidateCreateNotificationRequestDTO(dto);
            ValidateRole(role);

            var notification = MapCreateRequestToEntity(dto);
            var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

            await _notificationRepo.SendNotificationToRoleAsync(createdNotification.NtfId, role);

            // Clear global cache (role-based notifications affect multiple users)
            _cache.Remove("all_notifications");

            return MapToResponseDTO(createdNotification);
        }

        public async Task<NotificationResponseDTO> CreateAndSendToAllAsync(CreateNotificationRequestDTO dto)
        {
            ValidateCreateNotificationRequestDTO(dto);

            var notification = MapCreateRequestToEntity(dto);
            var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

            var allAccountIds = await _context.Accounts.Select(a => a.AccId).ToListAsync();

            // Use parallel processing for better performance when sending to many accounts
            var tasks = allAccountIds.Select(accId => 
                _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, accId));
            
            await Task.WhenAll(tasks);

            // Clear all notification caches since this affects all users
            var allCacheKeys = new List<string> { "all_notifications" };
            foreach (var accId in allAccountIds)
            {
                allCacheKeys.Add($"personal_notifications_{accId}");
                allCacheKeys.Add($"unread_notifications_{accId}");
                allCacheKeys.Add($"unread_count_{accId}");
            }

            foreach (var key in allCacheKeys)
            {
                _cache.Remove(key);
            }

            return MapToResponseDTO(createdNotification);
        }
    }
}