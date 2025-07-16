using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.AppointmentDTO;
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
        Task<AppointmentResponseDTO> CreateAppointmentAsync(CreateAppointmentRequestDTO appointment, int accId);
        Task<AppointmentResponseDTO?> GetAppointmentByIdAsync(int id);
        Task<AppointmentResponseDTO> UpdateAppointmentByIdAsync(int id, AppointmentRequestDTO appointment);
        Task<bool> DeleteAppointmentByIdAsync(int id);
        Task<AppointmentResponseDTO> ChangeAppointmentStatusAsync(int id, byte status);
        Task<List<AppointmentResponseDTO>> GetAppointmentsByAccountIdAsync(int accountId, byte role);
        Task<AppointmentResponseDTO> UpdateAppointmentRequestAsync(int appointmentId, UpdateAppointmentRequestDTO appointment, int accId);
        Task<List<AppointmentResponseDTO>> GetAllPersonalAppointmentsAsync(int accId);
        Task<AppointmentResponseDTO> CompleteAppointmentAsync(int appointmentId, CompleteAppointmentDTO dto, int accId);
        Task<AppointmentResponseDTO> GetPersonalAppointmentByIdAsync(int accId, int appointmentId);
        Task<AppointmentResponseDTO> ChangePersonalAppointmentStatusAsync(int accId, int appointmentId, byte status);
    }
}
