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
    public class ManagerDashboardDAO : BaseDashboardDAO
    {
        public ManagerDashboardDAO(HivSystemApiContext context) : base(context)
        {
        }

        public async Task<ManagerDashboardStats> GetManagerDashboardStatsAsync(DateTime today)
        {
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var stats = new ManagerDashboardStats
            {
                TotalDoctors = await GetTotalDoctorsAsync(),
                TotalStaff = await GetTotalStaffAsync(),
                TotalPatients = await GetTotalPatientsAsync(),
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.ApmtDate == DateOnly.FromDateTime(today)),
                MonthlyAppointments = await _context.Appointments
                    .CountAsync(a => a.ApmtDate >= DateOnly.Parse(startOfMonth.ToString()) && a.ApmtDate <= DateOnly.Parse(endOfMonth.ToString())),
                PendingTests = await _context.TestResults
                    .CountAsync(trs => trs.TestDate == default),
                MonthlyRevenue = await GetMonthlyRevenueAsync(startOfMonth, endOfMonth),
                DoctorPerformance = await _context.Doctors
                    .Select(d => new
                    {
                        DoctorId = d.DctId,
                        DoctorName = d.Acc != null ? d.Acc.Fullname : "",
                        AppointmentCount = d.Appointments.Count(),
                        PatientCount = d.Appointments.Select(a => a.PtnId).Distinct().Count()
                    })
                    .OrderByDescending(d => d.AppointmentCount)
                    .Take(5)
                    .Cast<dynamic>()
                    .ToListAsync(),
                StaffPerformance = await _context.Staff
                    .Select(s => new
                    {
                        StaffId = s.StfId,
                        StaffName = s.Acc != null ? s.Acc.Fullname : "",
                        TestResultCount = s.ComponentTestResults.Count(),
                        ServiceCount = s.ComponentTestResults.Count()
                    })
                    .OrderByDescending(s => s.TestResultCount)
                    .Take(5)
                    .Cast<dynamic>()
                    .ToListAsync(),
                ServiceUtilization = await _context.MedicalServices
                    .GroupBy(ms => ms.ServiceName)
                    .Select(g => new
                    {
                        ServiceName = g.Key,
                        UtilizationCount = g.Count(),
                        Revenue = g.SelectMany(ms => ms.Payments).Sum(p => p.Amount)
                    })
                    .OrderByDescending(g => g.UtilizationCount)
                    .Take(5)
                    .Cast<dynamic>()
                    .ToListAsync()
            };

            return stats;
        }

        public async Task<DashboardChart> GetManagerServiceChartAsync()
        {
            var utilization = await _context.MedicalServices
                .GroupBy(ms => ms.ServiceName)
                .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = utilization.Select(u => u.ServiceName).ToList();
            var data = utilization.Select(u => (object)u.Count).ToList();

            return new DashboardChart
            {
                ChartType = "bar",
                Title = "Service Utilization",
                Labels = labels,
                Datasets = new List<DashboardDataset>
                {
                    new DashboardDataset
                    {
                        Label = "Services",
                        Data = data,
                        BackgroundColor = "#FF6384",
                        BorderColor = "#FF6384",
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
