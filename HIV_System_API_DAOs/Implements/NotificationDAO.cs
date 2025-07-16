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

            // Set default values if not provided
            if (notification.SendAt == null)
                notification.SendAt = DateTime.UtcNow;

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
            if (notification.NotificationAccounts != null && notification.NotificationAccounts.Any())
            {
                _context.NotificationAccounts.RemoveRange(notification.NotificationAccounts);
            }

            // Then remove the notification
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.NotificationAccounts)
                .ThenInclude(na => na.Acc)
                .FirstOrDefaultAsync(n => n.NtfId == id);
        }


        public async Task<Notification> SendNotificationToAccIdAsync(int ntfId, int accId)
        {
            // Find the notification
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NtfId == ntfId);

            if (notification == null)
                throw new ArgumentException($"Notification with ID {ntfId} not found", nameof(ntfId));

            // Check if account exists
            var accountExists = await _context.Accounts.AnyAsync(a => a.AccId == accId);
            if (!accountExists)
                throw new ArgumentException($"Account with ID {accId} not found", nameof(accId));

            // Check if notification already sent to this account
            var existingMapping = await _context.NotificationAccounts
                .FirstOrDefaultAsync(na => na.NtfId == ntfId && na.AccId == accId);

            if (existingMapping != null)
            {
                // Already sent, just return the notification
                return notification;
            }

            // Create notification-account mapping
            var notificationAccount = new NotificationAccount
            {
                NtfId = ntfId,
                AccId = accId,
                IsRead = false
            };

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
                throw new ArgumentException($"Notification with ID {ntfId} not found", nameof(ntfId));

            // Get all accounts with the specified role
            var accounts = await _context.Accounts
                .Where(a => a.Roles == role)
                .Select(a => a.AccId)
                .ToListAsync();

            if (!accounts.Any())
                throw new ArgumentException($"No accounts found with role {role}", nameof(role));

            // Get existing mappings to avoid duplicates
            var existingMappings = await _context.NotificationAccounts
                .Where(na => na.NtfId == ntfId && accounts.Contains(na.AccId))
                .Select(na => na.AccId)
                .ToListAsync();

            // Create notification-account mappings for accounts that don't have them yet
            var newMappings = accounts
                .Where(accId => !existingMappings.Contains(accId))
                .Select(accId => new NotificationAccount
                {
                    NtfId = ntfId,
                    AccId = accId,
                    IsRead = false
                })
                .ToList();

            if (newMappings.Any())
            {
                await _context.NotificationAccounts.AddRangeAsync(newMappings);
                await _context.SaveChangesAsync();
            }

            return notification;
        }

        public async Task<bool> UpdateNotificationByIdAsync(Notification notification)
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

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications
                .Include(n => n.NotificationAccounts)
                .ThenInclude(na => na.Acc)
                .OrderByDescending(n => n.SendAt)
                .ToListAsync();
        }

        public async Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Acc)
                .Include(na => na.Ntf)
                .Where(na => na.NtfId == ntfId)
                .OrderBy(na => na.Acc.Fullname)
                .OrderByDescending(na => na.Ntf.SendAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsByRecipientAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId)
                .Select(na => na.Ntf)
                .OrderByDescending(n => n.SendAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllPersonalNotificationsAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId)
                .Select(na => na.Ntf)
                .OrderByDescending(n => n.SendAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllUnreadNotificationsAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId && na.IsRead == false)
                .Select(na => na.Ntf)
                .OrderByDescending(n => n.SendAt)
                .ToListAsync();
        }

        public async Task<Notification> ViewNotificationAsync(int ntfId, int accId)
        {
            var notificationAccount = await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .FirstOrDefaultAsync(na => na.NtfId == ntfId && na.AccId == accId);

            if (notificationAccount == null)
                throw new ArgumentException($"Notification {ntfId} not found for account {accId}");

            // Mark as read if not already read
            if (!notificationAccount.IsRead)
            {
                notificationAccount.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return notificationAccount.Ntf;
        }

        public async Task<bool> MarkNotificationAsReadAsync(int ntfId, int accId)
        {
            var notificationAccount = await _context.NotificationAccounts
                .FirstOrDefaultAsync(na => na.NtfId == ntfId && na.AccId == accId);

            if (notificationAccount == null)
                return false;

            if (!notificationAccount.IsRead)
            {
                notificationAccount.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int accId)
        {
            var unreadNotifications = await _context.NotificationAccounts
                .Where(na => na.AccId == accId && na.IsRead == false)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Where(na => na.AccId == accId && na.IsRead == false)
                .CountAsync();
        }

        public async Task<List<NotificationAccount>> GetPersonalNotificationAccountsAsync(int accId)
        {
            return await _context.NotificationAccounts
                .Include(na => na.Ntf)
                .Where(na => na.AccId == accId)
                .OrderByDescending(na => na.Ntf.SendAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteNotificationForAccountAsync(int ntfId, int accId)
        {
            var notificationAccount = await _context.NotificationAccounts
                .FirstOrDefaultAsync(na => na.NtfId == ntfId && na.AccId == accId);

            if (notificationAccount == null)
                return false;

            _context.NotificationAccounts.Remove(notificationAccount);

            // Check if this was the last recipient for this notification
            var otherRecipients = await _context.NotificationAccounts
                .Where(na => na.NtfId == ntfId && na.AccId != accId)
                .AnyAsync();

            // If no other recipients, delete the notification itself
            if (!otherRecipients)
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NtfId == ntfId);
                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
