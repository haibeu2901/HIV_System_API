using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class NotificationRepo : INotificationRepo
    {
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            return await NotificationDAO.Instance.CreateNotificationAsync(notification);
        }

        public async Task<bool> DeleteNotificationByIdAsync(int id)
        {
            return await NotificationDAO.Instance.DeleteNotificationByIdAsync(id);
        }

        public async Task<bool> DeleteNotificationForAccountAsync(int ntfId, int accId)
        {
            return await NotificationDAO.Instance.DeleteNotificationForAccountAsync(ntfId, accId);
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await NotificationDAO.Instance.GetAllNotificationsAsync();
        }

        public async Task<List<Notification>> GetAllPersonalNotificationsAsync(int accId)
        {
            return await NotificationDAO.Instance.GetAllPersonalNotificationsAsync(accId);
        }

        public async Task<List<Notification>> GetAllUnreadNotificationsAsync(int accId)
        {
            return await NotificationDAO.Instance.GetAllUnreadNotificationsAsync(accId);
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await NotificationDAO.Instance.GetNotificationByIdAsync(id);
        }

        public async Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId)
        {
            return await NotificationDAO.Instance.GetNotificationRecipientsAsync(ntfId);
        }

        public async Task<List<Notification>> GetNotificationsByRecipientAsync(int accId)
        {
            return await NotificationDAO.Instance.GetNotificationsByRecipientAsync(accId);
        }

        public async Task<List<NotificationAccount>> GetPersonalNotificationAccountsAsync(int accId)
        {
            return await NotificationDAO.Instance.GetPersonalNotificationAccountsAsync(accId);
        }

        public async Task<int> GetUnreadNotificationCountAsync(int accId)
        {
            return await NotificationDAO.Instance.GetUnreadNotificationCountAsync(accId);
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int accId)
        {
            return await NotificationDAO.Instance.MarkAllNotificationsAsReadAsync(accId);
        }

        public async Task<bool> MarkNotificationAsReadAsync(int ntfId, int accId)
        {
            return await NotificationDAO.Instance.MarkNotificationAsReadAsync(ntfId, accId);
        }

        public async Task<Notification> SendNotificationToAccIdAsync(int ntfId, int accId)
        {
            return await NotificationDAO.Instance.SendNotificationToAccIdAsync(ntfId, accId);
        }

        public Task<Notification> SendNotificationToRoleAsync(int ntfId, byte role)
        {
            return NotificationDAO.Instance.SendNotificationToRoleAsync(ntfId, role);
        }

        public async Task<bool> UpdateNotificationByIdAsync(Notification notification)
        {
            return await NotificationDAO.Instance.UpdateNotificationByIdAsync(notification);
        }

        public async Task<Notification> ViewNotificationAsync(int ntfId, int accId)
        {
            return await NotificationDAO.Instance.ViewNotificationAsync(ntfId, accId);
        }
    }
}
