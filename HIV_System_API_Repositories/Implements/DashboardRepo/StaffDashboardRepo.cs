using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements.DashboardRepo
{
    public class StaffDashboardRepo : BaseDashboardRepo
    {
        private readonly StaffDashboardDAO _staffDao;

        public StaffDashboardRepo(StaffDashboardDAO staffDao, BaseDashboardDAO baseDao)
            : base(baseDao)
        {
            _staffDao = staffDao;
        }

        public async Task<StaffDashboardStats> GetStaffDashboardStatsAsync(int staffId, DateTime today)
        {
            return await _staffDao.GetStaffDashboardStatsAsync(staffId, today);
        }

        //public async Task<List<DashboardAlert>> GetStaffAlertsAsync(int staffId, DateTime today)
        //{
        //    return await _staffDao.GetStaffAlertsAsync(staffId, today);
        //}
    }
}
