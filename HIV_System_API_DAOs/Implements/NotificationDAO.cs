using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_DAOs.Implements
{
    public class NotificationDAO : INotificationDAO
    {
        private readonly HivSystemApiContext _context;
        private static NotificationDAO? _instance;

        public NotificationDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static NotificationDAO Instance
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
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;

        }

        public async Task<bool> DeleteNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.NotificationAccounts)
                .FirstOrDefaultAsync(n => n.NtfId == id);
            
            if (notification == null)
                return false;

            // Remove related Notification_Account records first
            if (notification.NotificationAccounts != null)
            {
                _context.NotificationAccounts.RemoveRange(notification.NotificationAccounts);
            }

            // Then remove the notification
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NtfId == id);

            if (notification == null)
                throw new ArgumentException("Notification not found", nameof(id));

            return notification;
        }


        public async Task<Notification> SendNotificationToAccIdAsync(int ntfId, int accId)
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

            return notification;
        }

        public async Task<Notification> SendNotificationToRoleAsync(int ntfId, byte role)
        {
            // Find the notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NtfId == ntfId);
            if (notification == null)
                throw new ArgumentException("Notification not found", nameof(ntfId));

            // Get all accounts with the specified role
            var accounts = await _context.Accounts
                .Where(a => a.Roles == role)
                .Select(a => a.AccId)
                .ToListAsync();

            if (accounts.Count == 0)
                throw new ArgumentException("No accounts found with the specified role", nameof(role));

            // Create notification-account mappings
            var notificationAccounts = accounts.Select(accId => new NotificationAccount
            {
                NtfId = ntfId,
                AccId = accId
            });

            // Add all mappings in one go
            await _context.NotificationAccounts.AddRangeAsync(notificationAccounts);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> UpdateNotificationByIdAsync(Notification notification)
        {
            try
            {
                if (notification == null)
                    throw new ArgumentNullException(nameof(notification));

                var existingNotification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NtfId == notification.NtfId);
                
                if (existingNotification == null)
                    return false;

                // Update properties
                existingNotification.NotiMessage = notification.NotiMessage;
                existingNotification.SendAt = notification.SendAt;
                existingNotification.NotiType = notification.NotiType;

                await _context.SaveChangesAsync();
                
                // Detach the entity from the context to prevent tracking issues
                _context.Entry(existingNotification).State = EntityState.Detached;
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Notification>> GetAllNotification()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Acc)
                .Where(na => na.NtfId == ntfId)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsByRecipientAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId)
                .Select(na => na.Ntf)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllPersonalNotificationsAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId)
                .Select(na => na.Ntf)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllUnreadNotificationsAsync(int accId)
        {
            // Check if NotificationAccount has a property named 'IsRead'
            // If not, this will throw at runtime; otherwise, it will work as intended.
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId)
                .Where(na => na.IsRead == false)
                .Select(na => na.Ntf)
                .ToListAsync();
        }

        
    }
}
