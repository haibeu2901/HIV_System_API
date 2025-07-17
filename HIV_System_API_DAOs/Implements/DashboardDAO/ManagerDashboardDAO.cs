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
            var todayDateOnly = DateOnly.FromDateTime(today);
            var startOfMonthDateOnly = DateOnly.FromDateTime(startOfMonth);
            var endOfMonthDateOnly = DateOnly.FromDateTime(endOfMonth);

            // Execute all count queries in parallel for better performance
            var totalDoctorsTask = GetTotalDoctorsAsync();
            var totalStaffTask = GetTotalStaffAsync();
            var totalPatientsTask = GetTotalPatientsAsync();
            var todayAppointmentsTask = _context.Appointments.CountAsync(a => a.ApmtDate == todayDateOnly);
            var monthlyAppointmentsTask = _context.Appointments.CountAsync(a => a.ApmtDate >= startOfMonthDateOnly && a.ApmtDate <= endOfMonthDateOnly);
            var pendingTestsTask = _context.TestResults.CountAsync(trs => trs.TestDate == default(DateOnly));
            var monthlyRevenueTask = GetMonthlyRevenueAsync(startOfMonth, endOfMonth);

            // Execute performance queries in parallel
            var doctorPerformanceTask = GetDoctorPerformanceAsync();
            var staffPerformanceTask = GetStaffPerformanceAsync();
            var serviceUtilizationTask = GetServiceUtilizationAsync();

            // Wait for all tasks to complete
            await Task.WhenAll(
                totalDoctorsTask, totalStaffTask, totalPatientsTask, 
                todayAppointmentsTask, monthlyAppointmentsTask, pendingTestsTask, monthlyRevenueTask,
                doctorPerformanceTask, staffPerformanceTask, serviceUtilizationTask);

            var stats = new ManagerDashboardStats
            {
                TotalDoctors = await totalDoctorsTask,
                TotalStaff = await totalStaffTask,
                TotalPatients = await totalPatientsTask,
                TodayAppointments = await todayAppointmentsTask,
                MonthlyAppointments = await monthlyAppointmentsTask,
                PendingTests = await pendingTestsTask,
                MonthlyRevenue = await monthlyRevenueTask,
                DoctorPerformance = await doctorPerformanceTask,
                StaffPerformance = await staffPerformanceTask,
                ServiceUtilization = await serviceUtilizationTask
            };
            return stats;
        }

        private async Task<List<dynamic>> GetDoctorPerformanceAsync()
        {
            return await _context.Doctors
                .Include(d => d.Acc)
                .Select(d => new
                {
                    DoctorId = d.DctId,
                    DoctorName = d.Acc != null ? d.Acc.Fullname ?? string.Empty : string.Empty,
                    AppointmentCount = d.Appointments.Count(),
                    PatientCount = d.Appointments.Select(a => a.PtnId).Distinct().Count()
                })
                .OrderByDescending(d => d.AppointmentCount)
                .Take(5)
                .Cast<dynamic>()
                .ToListAsync();
        }

        private async Task<List<dynamic>> GetStaffPerformanceAsync()
        {
            return await _context.Staff
                .Include(s => s.Acc)
                .Select(s => new
                {
                    StaffId = s.StfId,
                    StaffName = s.Acc != null ? s.Acc.Fullname ?? string.Empty : string.Empty,
                    TestResultCount = s.ComponentTestResults.Count(),
                    ServiceCount = s.ComponentTestResults.Count()
                })
                .OrderByDescending(s => s.TestResultCount)
                .Take(5)
                .Cast<dynamic>()
                .ToListAsync();
        }

        private async Task<List<dynamic>> GetServiceUtilizationAsync()
        {
            return await _context.MedicalServices
                .GroupBy(ms => ms.ServiceName)
                .Select(g => new
                {
                    ServiceName = g.Key ?? string.Empty,
                    UtilizationCount = g.Count(),
                    Revenue = g.SelectMany(ms => ms.Payments).Sum(p => p.Amount)
                })
                .OrderByDescending(g => g.UtilizationCount)
                .Take(5)
                .Cast<dynamic>()
                .ToListAsync();
        }

        public async Task<DashboardChart> GetManagerServiceChartAsync()
        {
            var utilization = await _context.MedicalServices
                .GroupBy(ms => ms.ServiceName)
                .Select(g => new { ServiceName = g.Key ?? string.Empty, Count = g.Count() })
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
