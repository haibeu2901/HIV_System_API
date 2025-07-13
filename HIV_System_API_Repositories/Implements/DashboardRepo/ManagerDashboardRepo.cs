using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements.DashboardRepo
{
    public class ManagerDashboardRepo : BaseDashboardRepo
    {
        private readonly ManagerDashboardDAO _ManagerDAO;

        public ManagerDashboardRepo(ManagerDashboardDAO ManagerDAO, BaseDashboardDAO baseDAO)
            : base(baseDAO)
        {
            _ManagerDAO = ManagerDAO;
        }

        public async Task<ManagerDashboardStats> GetManagerDashboardStatsAsync(DateTime today)
        {
            return await _ManagerDAO.GetManagerDashboardStatsAsync(today);
        }

        public async Task<DashboardChart> GetManagerServiceChartAsync()
        {
            return await _ManagerDAO.GetManagerServiceChartAsync();
        }
    }
}
