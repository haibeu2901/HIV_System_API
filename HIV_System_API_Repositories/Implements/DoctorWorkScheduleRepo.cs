using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class DoctorWorkScheduleRepo : IDoctorWorkScheduleRepo
    {
        public async Task<DoctorWorkSchedule> CreateDoctorWorkScheduleAsync(DoctorWorkSchedule doctorWorkSchedule)
        {
            return await DoctorWorkScheduleDAO.Instance.CreateDoctorWorkScheduleAsync(doctorWorkSchedule);
        }

        public async Task<bool> DeleteDoctorWorkScheduleAsync(int id)
        {
            return await DoctorWorkScheduleDAO.Instance.DeleteDoctorWorkScheduleAsync(id);
        }

        public async Task<DoctorWorkSchedule?> GetDoctorWorkScheduleByIdAsync(int id)
        {
            return await DoctorWorkScheduleDAO.Instance.GetDoctorWorkScheduleByIdAsync(id);
        }

        public async Task<List<DoctorWorkSchedule>> GetDoctorWorkSchedulesAsync()
        {
            return await DoctorWorkScheduleDAO.Instance.GetDoctorWorkSchedulesAsync();
        }

        public async Task<List<DoctorWorkSchedule>> GetDoctorWorkSchedulesByDoctorIdAsync(int doctorId)
        {
            return await DoctorWorkScheduleDAO.Instance.GetDoctorWorkSchedulesByDoctorIdAsync(doctorId);
        }

        public Task<List<DoctorWorkSchedule>> GetPersonalWorkSchedulesAsync(int doctorId)
        {
            return DoctorWorkScheduleDAO.Instance.GetPersonalWorkSchedulesAsync(doctorId);
        }

        public async Task<DoctorWorkSchedule> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkSchedule doctorWorkSchedule)
        {
            return await DoctorWorkScheduleDAO.Instance.UpdateDoctorWorkScheduleAsync(id, doctorWorkSchedule);
        }
    }
}
