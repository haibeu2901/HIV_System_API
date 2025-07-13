using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements.DashboardRepo
{
    public class DoctorDashboardRepo : BaseDashboardRepo
    {
        private readonly DoctorDashboardDAO _doctorDao;

        public DoctorDashboardRepo(DoctorDashboardDAO doctorDao, BaseDashboardDAO baseDao)
            : base(baseDao)
        {
            _doctorDao = doctorDao;
        }

        public async Task<DoctorDashboardStats> GetDoctorDashboardStatsAsync(int doctorId, DateTime today)
        {
            return await _doctorDao.GetDoctorDashboardStatsAsync(doctorId, today);
        }

        //public async Task<List<DashboardAlert>> GetDoctorAlertsAsync(int doctorId, DateTime today)
        //{
        //    return await _doctorDao.GetDoctorAlertsAsync(doctorId, today);
        //}
    }
}
