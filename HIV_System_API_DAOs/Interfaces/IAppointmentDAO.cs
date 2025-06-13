using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
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
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<Appointment> UpdateAppointmentByIdAsync(Appointment appointment);
        Task<bool> DeleteAppointmentByIdAsync(int id);
        Task<Appointment> ChangeAppointmentStatusAsync(int id, byte status);
        Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId);
    }
}
