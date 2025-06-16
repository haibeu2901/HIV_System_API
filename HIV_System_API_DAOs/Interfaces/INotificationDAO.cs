using HIV_System_API_BOs;
using HIV_System_API_DTOs.NotificationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface INotificationDAO
    {
        Task<List<Notification>> GetAllNotification();
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> UpdateNotificationByIdAsync(Notification notification);
        Task<bool> DeleteNotificationByIdAsync(int id);
        Task<Notification> SendNotificationToRoleAsync(int ntfId, byte role);
        Task<Notification> SendNotificationToAccIdAsync(int ntfId, int accId);
        Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId);
        Task<List<Notification>> GetNotificationsByRecipientAsync(int accId);
        Task<List<Notification>> GetAllPersonalNotificationsAsync(int accId);
    }
}
