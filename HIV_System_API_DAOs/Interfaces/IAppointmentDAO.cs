using HIV_System_API_BOs;
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
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<bool> UpdateAppointmentByIdAsync(int id);
        Task<bool> DeleteAppointmentByIdAsync(int id);
        Task<bool> ChangeAppointmentStatusAsync(int id, byte status);
    }
}
