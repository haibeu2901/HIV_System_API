using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.NotificationDTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class NotificationDAO : INotificationDAO
    {
        private readonly HivSystemContext _context;
        private static INotificationDAO _instance;

        public NotificationDAO()
        {
            _context = new HivSystemContext();
        }

        public static INotificationDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NotificationDAO();
                }
                return _instance;
            }
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            if(notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;

        }

        public async Task<bool> DeleteNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NtfId == id);
            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<NotificationDTO> GetNotificationByAccId(int accId)
        {
            var notification = await _context.NotificationAccounts
                .Include(n => n.Ntf) // Include the related Notification entity
                .FirstOrDefaultAsync(n => n.AccId == accId);
                
            if (notification == null)
                return null;
                
            return new NotificationDTO
            {
                NotiMessage = notification.Ntf.NotiMessage,
                SendAt = notification.Ntf.SendAt,
                NotiType = notification.Ntf.NotiType,
            };
        }

        public async Task<NotificationDTO> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NtfId == id);

            if(notification == null)
                return null;

            return new NotificationDTO
            {
                NtfId = notification.NtfId,
                NotiMessage = notification.NotiMessage,
                SendAt = notification.SendAt,
                NotiType = notification.NotiType
            };
        }

        
        public async Task<NotificationDTO> SendNotificationByAccIdAsync(int ntfId, int accId)
        {
            // Find the notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NtfId == ntfId);
                
            if (notification == null)
                throw new ArgumentException("Notification not found", nameof(ntfId));

            // Create notification-account mapping
            var notificationAccount = new NotificationAccount
            {
                NtfId = ntfId,
                AccId = accId
            };

            // Add and save
            await _context.NotificationAccounts.AddAsync(notificationAccount);
            await _context.SaveChangesAsync();

            // Return notification details
            return new NotificationDTO
            {
                NotiMessage = notification.NotiMessage,
                SendAt = notification.SendAt,
                NotiType = notification.NotiType
            };
        }

        public async Task<NotificationDTO> SendNotificationByRoleAsync(int ntfId, byte role)
        {
            //Find the notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NtfId == ntfId);
            if(notification == null)
                throw new ArgumentException("Notification not found", nameof(ntfId));

            // Get all accounts with the specified role
            var accounts = await _context.Accounts
                .Where(a => a.Roles == role)
                .Select(a => a.AccId)
                .ToListAsync();

            if (accounts.Count == 0)
                throw new ArgumentException("No accounts found with the specified role", nameof(role));

            // Create notification-account mappings for each account
            foreach (var accId in accounts)
            {
                var notificationAccount = new NotificationAccount
                {
                    NtfId = ntfId,
                    AccId = accId
                };
                await _context.NotificationAccounts.AddAsync(notificationAccount);
                await _context.SaveChangesAsync();
            }
            return new NotificationDTO {
                NotiMessage = notification.NotiMessage,
                SendAt = notification.SendAt,
                NotiType = notification.NotiType
            };
        }

        public async Task<bool> UpdateNotificationByIdAsync(Notification notification)
        {
            try
            {
                if (notification == null)
                    throw new ArgumentNullException(nameof(notification));
                var existingNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.NtfId == notification.NtfId);
                if (existingNotification == null)
                    return false;
                existingNotification.NotiMessage = notification.NotiMessage;
                existingNotification.SendAt = notification.SendAt;
                existingNotification.NotiType = notification.NotiType;
                _context.Notifications.Update(existingNotification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<List<NotificationDTO>> GetAllNotification()
        {
            var notifications = _context.Notifications
                .Select(n => new NotificationDTO
                {
                    NtfId = n.NtfId,
                    NotiMessage = n.NotiMessage,
                    SendAt = n.SendAt,
                    NotiType = n.NotiType
                }).ToListAsync();
            return notifications;
        }
    }
}
