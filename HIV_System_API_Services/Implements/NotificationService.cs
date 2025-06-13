using HIV_System_API_BOs;
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
        private readonly INotificationRepo NotificationRepo;

        public NotificationService()
        {
            NotificationRepo = new NotificationRepo();
        }
        public Task<Notification> CreateNotificationAsync(Notification notification)
        {
            return NotificationRepo.CreateNotificationAsync(notification);
        }

        public Task<bool> DeleteNotificationByIdAsync(int id)
        {
            return NotificationRepo.DeleteNotificationByIdAsync(id);
        }

        public Task<List<NotificationDTO>> GetAllNotifications()
        {
            return NotificationRepo.GetAllNotification();
        }

        public Task<NotificationDTO> GetNotificationByAccId(int accId)
        {
            return NotificationRepo.GetNotificationByAccId(accId);
        }

        public Task<NotificationDTO> GetNotificationByIdAsync(int id)
        {
            return NotificationRepo.GetNotificationByIdAsync(id);
        }

        public Task<NotificationDTO> SendNotificationByAccIdAsync(int ntfId, int accId)
        {
            return NotificationRepo.SendNotificationByAccIdAsync(ntfId, accId);
        }

        public Task<NotificationDTO> SendNotificationByRoleAsync(int ntfId, byte role)
        {
            return NotificationRepo.SendNotificationByRoleAsync(ntfId, role);
        }

        public Task<bool> UpdateNotificationByIdAsync(Notification notification)
        {
            return NotificationRepo.UpdateNotificationByIdAsync(notification);
        }
    }
}
