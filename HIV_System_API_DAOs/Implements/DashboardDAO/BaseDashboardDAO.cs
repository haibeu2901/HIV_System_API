using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements.DashboardDAO
{
    public class BaseDashboardDAO
    {
        protected readonly HivSystemApiContext _context;

        public BaseDashboardDAO(HivSystemApiContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _context.Accounts.CountAsync();
        }

        public async Task<int> GetTotalPatientsAsync()
        {
            return await _context.Accounts.CountAsync(a => a.Roles == 3);
        }

        public async Task<int> GetTotalDoctorsAsync()
        {
            return await _context.Accounts.CountAsync(a => a.Roles == 2);
        }

        public async Task<int> GetTotalStaffAsync()
        {
            return await _context.Accounts.CountAsync(a => a.Roles == 4);
        }

        public async Task<int> GetTotalManagerAsync()
        {
            return await _context.Accounts.CountAsync(a => a.Roles == 5);
        }

        public async Task<int> GetTotalAppointmentsAsync()
        {
            return await _context.Appointments.CountAsync();
        }

        public async Task<int> GetPendingAppointmentsAsync(DateTime today)
        {
            return await _context.Appointments
                .Where(a => a.ApmtDate >= DateOnly.FromDateTime(today.Date))
                .CountAsync();
        }

        public async Task<int> GetTotalServicesAsync()
        {
            return await _context.MedicalServices.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Payments
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(DateTime startOfMonth, DateTime endOfMonth)
        {
            return await _context.Payments
                .Where(p => p.PaymentDate>= startOfMonth && p.PaymentDate <= endOfMonth)
                .SumAsync(p => p.Amount);
        }

        public async Task<List<dynamic>> GetRecentActivitiesAsync(int limit = 5)
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.SendAt)
                .Take(limit)
                .Select(n => new
                {
                    ActivityType = n.NotiType,
                    Description = n.NotiMessage,
                    CreatedAt = n.SendAt
                })
                .ToListAsync();

            return notifications.Cast<dynamic>().ToList();
        }

        //public async Task<List<DashboardAlert>> GetDashboardAlertsAsync(int userId, DateTime today)
        //{
        //    return await _context.NotificationAccounts
        //        .Where(na => na.AccId == userId && na.Ntf.SendAt >= today.AddDays(-7))
        //        .Select(na => new DashboardAlert
        //        {
        //            Type = na.Ntf.NotiType,
        //            Message = na.Ntf.NotiMessage,
        //            Priority = na.Ntf.NotiType == "Urgent" ? "High" : "Normal",
        //            CreatedAt = na.Ntf.SendAt.Value,
        //            ActionRequired = na.Ntf.NotiType == "Appointment" ? "View Appointment" : "Review",
        //            ActionUrl = na.Ntf.NotiType == "Appointment" ? $"/appointments/{na.NtfId}" : $"/notifications/{na.NtfId}"
        //        })
        //        .Take(3)
        //        .ToListAsync();
        //}

        public async Task<DashboardChart> GetUserDistributionChartAsync()
        {
            var distribution = await _context.Accounts
                .GroupBy(a => a.Roles)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = new List<string> { "Admin", "Doctor", "Patient", "Staff", "Manager" };
            var data = new List<object> { 0, 0, 0, 0, 0 };
            foreach (var item in distribution)
            {
                data[item.Role - 1] = item.Count;
            }

            return new DashboardChart
            {
                ChartType = "pie",
                Title = "User Distribution by Role",
                Labels = labels,
                Datasets = new List<DashboardDataset>
                {
                    new DashboardDataset
                    {
                        Label = "Users",
                        Data = data,
                        BackgroundColor = "#36A2EB",
                        BorderColor = "#36A2EB",
                        BorderWidth = 1,
                        Fill = false
                    }
                },
                Options = new Dictionary<string, object>
                {
                    { "responsive", true }
                }
            };
        }
    }
}
