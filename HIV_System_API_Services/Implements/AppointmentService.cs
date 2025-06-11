using HIV_System_API_BOs;
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
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepo _appointmentRepo;

        public AppointmentService()
        {
            _appointmentRepo = new AppointmentRepo();
        }

        public async Task<bool> ChangeAppointmentStatusAsync(int id, byte status)
        {
            return await _appointmentRepo.ChangeAppointmentStatusAsync(id, status);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            return await _appointmentRepo.CreateAppointmentAsync(appointment);
        }

        public async Task<bool> DeleteAppointmentByIdAsync(int id)
        {
            return await _appointmentRepo.DeleteAppointmentByIdAsync(id);
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _appointmentRepo.GetAllAppointmentsAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _appointmentRepo.GetAppointmentByIdAsync(id);
        }

        public async Task<bool> UpdateAppointmentByIdAsync(int id)
        {
            return await _appointmentRepo.UpdateAppointmentByIdAsync(id);
        }
    }
}
