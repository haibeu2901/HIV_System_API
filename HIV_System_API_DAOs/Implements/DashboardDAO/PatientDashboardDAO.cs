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
    public class PatientDashboardDAO : BaseDashboardDAO
    {
        public PatientDashboardDAO(HivSystemApiContext context) : base(context)
        {
        }

        public async Task<PatientDashboardStats> GetPatientDashboardStatsAsync(int patientId, DateTime today)
        {
            var todayDateOnly = DateOnly.FromDateTime(today);
            var thirtyDaysAgo = DateOnly.FromDateTime(today.AddDays(-30));

            var stats = new PatientDashboardStats
            {
                UpcomingAppointments = await _context.Appointments
                    .CountAsync(a => a.PtnId == patientId && a.ApmtDate >= todayDateOnly && a.ApmStatus == 1),
                TotalAppointments = await _context.Appointments
                    .CountAsync(a => a.PtnId == patientId),
                RecentTestResults = await _context.TestResults
                    .CountAsync(tr => tr.Pmr.PtnId == patientId && tr.TestDate >= thirtyDaysAgo),
                ActiveRegimens = await _context.PatientArvRegimen
                    .CountAsync(par => par.Pmr.PtnId == patientId),
                TotalPayments = await _context.Payments
                    .Where(p => p.Pmr.PtnId == patientId)
                    .SumAsync(p => p.Amount),
                NextAppointment = await _context.Appointments
                    .Where(a => a.PtnId == patientId && a.ApmtDate >= todayDateOnly && a.ApmStatus == 1)
                    .OrderBy(a => a.ApmtDate)
                    .ThenBy(a => a.ApmTime)
                    .Select(a => new
                    {
                        ApmId = a.ApmId,
                        ApmDate = a.ApmtDate,
                        DoctorName = a.Dct.Acc.Fullname ?? "Unknown",
                        Notes = a.Notes ?? ""
                    })
                    .FirstOrDefaultAsync<dynamic>(),
                RecentTestResultsList = await _context.TestResults
                    .Where(tr => tr.Pmr.PtnId == patientId)
                    .OrderByDescending(tr => tr.TestDate)
                    .Take(5)
                    .Select(tr => new
                    {
                        TrsId = tr.TrsId,
                        TestDate = tr.TestDate,
                        Notes = tr.Notes ?? ""
                    })
                    .ToListAsync<dynamic>(),
                CurrentMedications = await _context.PatientArvMedications
                    .Where(pam => pam.Par.Pmr.PtnId == patientId)
                    .OrderByDescending(pam => pam.Par.CreatedAt)
                    .Take(5)
                    .Select(pam => new
                    {
                        MedName = pam.Amd.MedName ?? "Unknown",
                        Dosage = pam.Amd.Dosage ?? "",
                        StartDate = pam.Par.CreatedAt
                    })
                    .ToListAsync<dynamic>()
            };

            return stats;
        }

        //public async Task<List<DashboardAlert>> GetPatientAlertsAsync(int patientId, DateTime today)
        //{
        //    return await GetDashboardAlertsAsync(patientId, today);
        //}
    }
}
