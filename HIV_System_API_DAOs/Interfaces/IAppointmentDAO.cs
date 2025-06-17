using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.AppointmentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IAppointmentDAO
    {
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequestDTO appointment, int accId);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<Appointment> UpdateAppointmentByIdAsync(int id, Appointment appointment);
        Task<bool> DeleteAppointmentByIdAsync(int id);
        Task<Appointment> ChangeAppointmentStatusAsync(int id, byte status);
        Task<List<Appointment>> GetAppointmentsByAccountIdAsync(int accountId, byte role);
        Task<Appointment> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequestDTO appointment, int accId);
        Task<List<Appointment>> GetAllPersonalAppointmentsAsync(int accId);
    }
}
