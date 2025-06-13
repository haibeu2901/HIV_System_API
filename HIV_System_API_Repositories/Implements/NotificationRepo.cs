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

        public Task<List<NotificationDTO>> GetAllNotification()
        {
            return NotificationDAO.Instance.GetAllNotification(); 
        }

        public Task<NotificationDTO> GetNotificationByAccId(int accId)
        {
            return NotificationDAO.Instance.GetNotificationByAccId(accId);
        }

        public Task<NotificationDTO> GetNotificationByIdAsync(int id)
        {
            return NotificationDAO.Instance.GetNotificationByIdAsync(id);
        }

        public Task<NotificationDTO> SendNotificationByAccIdAsync(int ntfId, int accId)
        {
            return NotificationDAO.Instance.SendNotificationByAccIdAsync(ntfId, accId);
        }

        public Task<NotificationDTO> SendNotificationByRoleAsync(int ntfId, byte role)
        {
            return NotificationDAO.Instance.SendNotificationByRoleAsync(ntfId, role);
        }

        public Task<bool> UpdateNotificationByIdAsync(Notification notification)
        {
            return NotificationDAO.Instance.UpdateNotificationByIdAsync(notification);
        }
    }
}
