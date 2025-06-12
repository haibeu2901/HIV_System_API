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
        Task<List<AppointmentDTO>> GetAllAppointmentsAsync();
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<AppointmentDTO> GetAppointmentByIdAsync(int id);
        Task<bool> UpdateAppointmentByIdAsync(Appointment appointment); 
        Task<bool> DeleteAppointmentByIdAsync(int id);
        Task<bool> ChangeAppointmentStatusAsync(int id, byte status);
    }
}
