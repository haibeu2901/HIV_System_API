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
        Task<List<NotificationResponseDTO>> GetAllNotificationsAsync();
        Task<NotificationResponseDTO?> GetNotificationByIdAsync(int id);
        Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationRequestDTO notification);
        Task<bool> UpdateNotificationByIdAsync(int id, UpdateNotificationRequestDTO notification);
        Task<bool> DeleteNotificationByIdAsync(int id);
        Task<NotificationResponseDTO> SendNotificationToRoleAsync(int ntfId, byte role);
        Task<NotificationResponseDTO> SendNotificationToAccIdAsync(int ntfId, int accId);
        Task<List<NotificationAccount>> GetNotificationRecipientsAsync(int ntfId);
        Task<List<NotificationResponseDTO>> GetNotificationsByRecipientAsync(int accId);
        Task<List<NotificationResponseDTO>> GetAllPersonalNotificationsAsync(int accId);
        Task<List<NotificationResponseDTO>> GetAllUnreadNotificationsAsync(int accId);
        Task<NotificationResponseDTO> ViewNotificationAsync(int ntfId, int accId);
        Task<bool> MarkNotificationAsReadAsync(int ntfId, int accId);
        Task<bool> MarkAllNotificationsAsReadAsync(int accId);
        Task<int> GetUnreadNotificationCountAsync(int accId);
        Task<List<NotificationAccount>> GetPersonalNotificationAccountsAsync(int accId);
        Task<bool> DeleteNotificationForAccountAsync(int ntfId, int accId);
        Task<NotificationDetailResponseDTO?> GetNotificationDetailsByIdAsync(int ntfId);
    }
}
