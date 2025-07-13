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
    public class StaffDashboardDAO : BaseDashboardDAO
    {
        public StaffDashboardDAO(HivSystemApiContext context) : base(context)
        {
        }

        public async Task<StaffDashboardStats> GetStaffDashboardStatsAsync(int staffId, DateTime today)
        {
            var todayDateOnly = DateOnly.FromDateTime(today);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var startOfMonthDateOnly = DateOnly.FromDateTime(startOfMonth);
            var endOfMonthDateOnly = DateOnly.FromDateTime(endOfMonth);

            var stats = new StaffDashboardStats
            {
                TodayTestResults = await _context.ComponentTestResults
                    .CountAsync(ctr => ctr.StfId == staffId && ctr.Trs.TestDate == todayDateOnly),
                MonthlyTestResults = await _context.ComponentTestResults
                    .CountAsync(ctr => ctr.StfId == staffId && ctr.Trs.TestDate >= startOfMonthDateOnly && ctr.Trs.TestDate <= endOfMonthDateOnly),
                TotalTestResults = await _context.ComponentTestResults
                    .CountAsync(ctr => ctr.StfId == staffId),
                PendingTests = await _context.ComponentTestResults
                    .CountAsync(ctr => ctr.StfId == staffId && ctr.ResultValue == null),
                BlogPosts = await _context.SocialBlogs
                    .CountAsync(sbl => sbl.StfId == staffId),
                RecentTestResults = await _context.ComponentTestResults
                    .Where(ctr => ctr.StfId == staffId)
                    .OrderByDescending(ctr => ctr.Trs.TestDate)
                    .Take(5)
                    .Select(ctr => new
                    {
                        TrsId = ctr.TrsId,
                        TestDate = ctr.Trs.TestDate,
                        CtrName = ctr.CtrName,
                        ResultValue = ctr.ResultValue,
                        Notes = ctr.Notes ?? ""
                    })
                    .ToListAsync<dynamic>(),
                WorkloadSummary = await _context.ComponentTestResults
                    .Where(ctr => ctr.StfId == staffId)
                    .GroupBy(ctr => ctr.CtrName)
                    .Select(g => new
                    {
                        TestName = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync<dynamic>()
            };

            return stats;
        }

        public async Task<List<DashboardAlert>> GetStaffAlertsAsync(int staffId, DateTime today)
        {
            return await GetDashboardAlertsAsync(staffId, today);
        }
    }
}
