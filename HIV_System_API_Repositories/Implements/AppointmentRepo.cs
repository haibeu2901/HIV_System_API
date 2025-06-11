using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class AppointmentRepo : IAppointmentRepo
    {
        public async Task<bool> ChangeAppointmentStatusAsync(int id, byte status)
        {
            return await AppointmentDAO.Instance.ChangeAppointmentStatusAsync(id, status);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            return await AppointmentDAO.Instance.CreateAppointmentAsync(appointment);
        }

        public async Task<bool> DeleteAppointmentByIdAsync(int id)
        {
            return await AppointmentDAO.Instance.DeleteAppointmentByIdAsync(id);
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await AppointmentDAO.Instance.GetAllAppointmentsAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await AppointmentDAO.Instance.GetAppointmentByIdAsync(id);
        }

        public async Task<bool> UpdateAppointmentByIdAsync(int id)
        {
            return await AppointmentDAO.Instance.UpdateAppointmentByIdAsync(id);
        }
    }
}
