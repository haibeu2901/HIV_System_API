using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements.DashboardRepo
{
    public class AdminDashboardRepo : BaseDashboardRepo
    {
        private readonly AdminDashboardDAO _adminDao;

        public AdminDashboardRepo(AdminDashboardDAO adminDao, BaseDashboardDAO baseDao) : base(baseDao)
        {
            _adminDao = adminDao;
        }

        public async Task<AdminDashboardStats> GetAdminDashboardStatsAsync(DateTime today)
        {
            return await _adminDao.GetAdminDashboardStatsAsync(today);
        }
    }
}
