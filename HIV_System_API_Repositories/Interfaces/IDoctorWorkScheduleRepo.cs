using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IDoctorWorkScheduleRepo
    {
        Task<List<DoctorWorkSchedule>> GetDoctorWorkSchedulesAsync();
        Task<DoctorWorkSchedule?> GetDoctorWorkScheduleByIdAsync(int id);
        Task<DoctorWorkSchedule> CreateDoctorWorkScheduleAsync(DoctorWorkSchedule doctorWorkSchedule);
        Task<bool> DeleteDoctorWorkScheduleAsync(int id);
        Task<DoctorWorkSchedule> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkSchedule doctorWorkSchedule);
    }
}
