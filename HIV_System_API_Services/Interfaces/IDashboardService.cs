using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardStats> GetAdminDashboardStatsAsync(int userId, DateOnly today);
        Task<DoctorDashboardStats> GetDoctorDashboardStatsAsync(int doctorId, DateTime today);
        Task<PatientDashboardStats> GetPatientDashboardStatsAsync(int patientId, DateTime today);
        Task<StaffDashboardStats> GetStaffDashboardStatsAsync(int staffId, DateTime today);
        Task<ManagerDashboardStats> GetManagerDashboardStatsAsync(int userId, DateTime today);
        Task<List<DashboardAlert>> GetDashboardAlertsAsync(int userId, string role, DateTime today);
        Task<DashboardChart> GetUserDistributionChartAsync(int userId);
        Task<DashboardChart> GetManagerServiceChartAsync(int userId);
    }
}
