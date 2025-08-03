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
    public class DoctorDashboardDAO : BaseDashboardDAO
    {
        public DoctorDashboardDAO(HivSystemApiContext context) : base(context)
        {
        }

        public async Task<DoctorDashboardStats> GetDoctorDashboardStatsAsync(int doctorId, DateTime today)
        {
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var stats = new DoctorDashboardStats
            {
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.DctId == doctorId && a.ApmtDate == DateOnly.FromDateTime(today.Date)),
                WeeklyAppointments = await _context.Appointments
                    .CountAsync(a => a.DctId == doctorId && a.ApmtDate >= DateOnly.FromDateTime(startOfWeek) && a.ApmtDate < DateOnly.FromDateTime(startOfWeek.AddDays(7))),
                MonthlyAppointments = await _context.Appointments
                    .CountAsync(a => a.DctId == doctorId && a.ApmtDate >= DateOnly.FromDateTime(startOfMonth) && a.ApmtDate <= DateOnly.FromDateTime(endOfMonth)),
                TotalPatients = await _context.Appointments
                    .Where(a => a.DctId == doctorId)
                    .Select(a => a.PtnId)
                    .Distinct()
                    .CountAsync(),
                UpcomingAppointments = await _context.Appointments
                    .CountAsync(a => a.DctId == doctorId && a.ApmtDate >= DateOnly.FromDateTime(today) && (a.ApmStatus == 2 || a.ApmStatus ==3)),
                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.DctId == doctorId && a.ApmtDate < DateOnly.FromDateTime(today) && a.ApmStatus == 5),
                TodaySchedule = await _context.Appointments
                    .Where(a => a.DctId == doctorId && a.ApmtDate == DateOnly.FromDateTime(today.Date))
                    .OrderBy(a => a.ApmtDate)
                    .Take(5)
                    .Select(a => new
                    {
                        ApmId = a.ApmId,
                        ApmtDate = a.ApmtDate,
                        ApmtTime = a.ApmTime,
                        PatientName = a.Ptn.Acc.Fullname,
                        Notes = a.Notes,
                        Status = a.ApmStatus
                    })
                    .ToListAsync<dynamic>(),
                RecentPatients = await _context.Appointments
                    .Where(a => a.DctId == doctorId)
                    .Where(a => a.ApmStatus == 5)
                    .OrderByDescending(a => a.ApmtDate)
                    .Select(a => new
                    {
                        PatientId = a.PtnId,
                        PatientName = a.Ptn.Acc.Fullname,
                        LastVisit = a.ApmtDate,
                        LastVisitTime = a.ApmTime
                    })
                    .Take(5)
                    .ToListAsync<dynamic>()
            };

            return stats;
        }

        //public async Task<List<DashboardAlert>> GetDoctorAlertsAsync(int doctorId, DateTime today)
        //{
        //    return await GetDashboardAlertsAsync(doctorId, today);
        //}
    }
}
