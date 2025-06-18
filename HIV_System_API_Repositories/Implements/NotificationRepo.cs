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

        public Task<bool> DeleteNotificationByIdAsync(int id)
        {
            return NotificationDAO.Instance.DeleteNotificationByIdAsync(id);
        }

        public Task<List<Notification>> GetAllNotification()
        {
            return NotificationDAO.Instance.GetAllNotification(); 
        }

        public async Task<List<Notification>> GetAllPersonalNotificationsAsync(int accId)
        {
            return await NotificationDAO.Instance.GetAllPersonalNotificationsAsync(accId);
        }

        public async Task<List<Notification>> GetAllUnreadNotificationsAsync(int accId)
        {
            return await NotificationDAO.Instance.GetAllUnreadNotificationsAsync(accId);
        }

        public Task<Notification> GetNotificationByIdAsync(int id)
        {
            return NotificationDAO.Instance.GetNotificationByIdAsync(id);
        }

        public Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId)
        {
            return NotificationDAO.Instance.GetNotificationRecipientsAsync(ntfId);
        }

        public Task<List<Notification>> GetNotificationsByRecipientAsync(int accId)
        {
            return NotificationDAO.Instance.GetNotificationsByRecipientAsync(accId);
        }

        public Task<Notification> SendNotificationToAccIdAsync(int ntfId, int accId)
        {
            return NotificationDAO.Instance.SendNotificationToAccIdAsync(ntfId, accId);
        }

        public Task<Notification> SendNotificationToRoleAsync(int ntfId, byte role)
        {
            return NotificationDAO.Instance.SendNotificationToRoleAsync(ntfId, role);
        }

        public Task<bool> UpdateNotificationByIdAsync(Notification notification)
        {
            return NotificationDAO.Instance.UpdateNotificationByIdAsync(notification);
        }

        
    }
}
