using HIV_System_API_BOs;
using HIV_System_API_DTOs.AppointmentDTO;
using HIV_System_API_DTOs.NotificationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDTO>> GetAllNotifications();
        Task<NotificationResponseDTO> GetNotificationByIdAsync(int id);
        Task<NotificationDetailResponseDTO> GetNotificationDetailsByIdAsync(int id);  // New method
        Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationRequestDTO notification);
        Task<bool> UpdateNotificationByIdAsync(int id, UpdateNotificationRequestDTO notification);
        Task<bool> DeleteNotificationByIdAsync(int id);
        Task<NotificationDetailResponseDTO> SendNotificationToRoleAsync(int ntfId, byte role);  // Changed return type
        Task<NotificationDetailResponseDTO> SendNotificationToAccIdAsync(int ntfId, int accId);  // Changed return type
        Task<List<NotificationResponseDTO>> GetNotificationsByRecipientAsync(int accId);  // New method
    }
}
