using HIV_System_API_BOs;
using HIV_System_API_DTOs.NotificationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface INotificationRepo
    {
        Task<List<NotificationDTO>> GetAllNotification();
        Task<NotificationDTO> GetNotificationByIdAsync(int id);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<NotificationDTO> GetNotificationByAccId(int accId);
        Task<bool> UpdateNotificationByIdAsync(Notification notification);
        Task<bool> DeleteNotificationByIdAsync(int id);
        Task<NotificationDTO> SendNotificationByRoleAsync(int ntfId, byte role);
        Task<NotificationDTO> SendNotificationByAccIdAsync(int ntfId, int accId);
    }
}
