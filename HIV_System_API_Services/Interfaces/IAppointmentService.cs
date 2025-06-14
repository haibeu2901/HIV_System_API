using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<AppointmentResponseDTO>> GetAllAppointmentsAsync();
        Task<AppointmentResponseDTO> CreateAppointmentAsync(AppointmentRequestDTO appointment);
        Task<AppointmentResponseDTO?> GetAppointmentByIdAsync(int id);
        Task<AppointmentResponseDTO> UpdateAppointmentByIdAsync(int id, AppointmentRequestDTO appointment);
        Task<bool> DeleteAppointmentByIdAsync(int id);
        Task<AppointmentResponseDTO> ChangeAppointmentStatusAsync(int id, byte status);
        Task<List<AppointmentResponseDTO>> GetAppointmentsByAccountIdAsync(int accId);
    }
}
