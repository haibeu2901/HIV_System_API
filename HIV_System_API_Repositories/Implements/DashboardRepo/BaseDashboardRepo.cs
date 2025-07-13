using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements.DashboardRepo
{
    public class BaseDashboardRepo
    {
        protected readonly BaseDashboardDAO _baseDao;

        public BaseDashboardRepo(BaseDashboardDAO baseDao)
        {
            _baseDao = baseDao;
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _baseDao.GetTotalUsersAsync();
        }

        public async Task<int> GetTotalPatientsAsync()
        {
            return await _baseDao.GetTotalPatientsAsync();
        }

        public async Task<int> GetTotalDoctorsAsync()
        {
            return await _baseDao.GetTotalDoctorsAsync();
        }

        public async Task<int> GetTotalStaffAsync()
        {
            return await _baseDao.GetTotalStaffAsync();
        }

        public async Task<int> GetTotalManagerAsync()
        {
            return await _baseDao.GetTotalManagerAsync();
        }

        public async Task<List<DashboardAlert>> GetDashboardAlertsAsync(int userId, DateTime today)
        {
            return await _baseDao.GetDashboardAlertsAsync(userId, today);
        }

        public async Task<DashboardChart> GetUserDistributionChartAsync()
        {
            return await _baseDao.GetUserDistributionChartAsync();
        }
    }
}
