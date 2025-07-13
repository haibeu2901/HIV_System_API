using HIV_System_API_BOs;
using HIV_System_API_DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements.DashboardDAO
{
    public class AdminDashboardDAO : BaseDashboardDAO
    {
        public AdminDashboardDAO(HivSystemApiContext context) : base(context)
        {
        }

        public async Task<AdminDashboardStats> GetAdminDashboardStatsAsync(DateTime today)
        {
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Fetch user distribution first to avoid LINQ translation issues
            var userDistributionData = await _context.Accounts
                .GroupBy(a => a.Roles)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            var stats = new AdminDashboardStats
            {
                TotalUsers = await GetTotalUsersAsync(),
                TotalPatients = await GetTotalPatientsAsync(),
                TotalDoctors = await GetTotalDoctorsAsync(),
                TotalStaff = await GetTotalStaffAsync(),
                TotalManager = await GetTotalManagerAsync(),
                TotalAppointments = await GetTotalAppointmentsAsync(),
                PendingAppointments = await GetPendingAppointmentsAsync(today),
                TotalServices = await GetTotalServicesAsync(),
                TotalRevenue = await GetTotalRevenueAsync(),
                MonthlyRevenue = await GetMonthlyRevenueAsync(startOfMonth, endOfMonth),
                UserDistribution = userDistributionData.ToDictionary(
                    g => g.Role switch
                    {
                        1 => "Admin",
                        2 => "Doctor",
                        3 => "Patient",
                        4 => "Staff",
                        5 => "Manager",
                        _ => "Unknown"
                    },
                    g => g.Count
                ),
                RecentActivities = await GetRecentActivitiesAsync()
            };

            return stats;
        }
    }
}
