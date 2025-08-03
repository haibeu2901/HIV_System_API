using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalStaff { get; set; }
        public int TotalManager { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int TotalServices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public Dictionary<string, int> UserDistribution { get; set; }
        public List<dynamic> RecentActivities { get; set; }
    }

    public class DoctorDashboardStats
    {
        public int TodayAppointments { get; set; }
        public int WeeklyAppointments { get; set; }
        public int MonthlyAppointments { get; set; }
        public int TotalPatients { get; set; }
        public int UpcomingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<dynamic> TodaySchedule { get; set; }
        public List<dynamic> RecentPatients { get; set; }
    }

    public class PatientDashboardStats
    {
        public int UpcomingAppointments { get; set; }
        public int TotalAppointments { get; set; }
        public int RecentTestResults { get; set; }
        public int ActiveRegimens { get; set; }
        public decimal TotalPayments { get; set; }
        public dynamic NextAppointment { get; set; }
        public List<dynamic> RecentTestResultsList { get; set; }
        public List<dynamic> CurrentMedications { get; set; }
    }

    public class StaffDashboardStats
    {
        public int TodayTestResults { get; set; }
        public int MonthlyTestResults { get; set; }
        public int TotalTestResults { get; set; }
        public int PendingTests { get; set; }
        public int BlogPosts { get; set; }
        public List<dynamic> RecentTestResults { get; set; }
        public List<dynamic> WorkloadSummary { get; set; }
    }

    public class ManagerDashboardStats
    {
        public int TotalDoctors { get; set; }
        public int TotalStaff { get; set; }
        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public int MonthlyAppointments { get; set; }
        public int PendingTests { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<dynamic> DoctorPerformance { get; set; }
        public List<dynamic> StaffPerformance { get; set; }
        public List<dynamic> ServiceUtilization { get; set; }
    }

    public class DashboardAlert
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ActionRequired { get; set; }
        public string ActionUrl { get; set; }
    }

    public class DashboardMetric
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Unit { get; set; }
        public double? PercentageChange { get; set; }
        public string Trend { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class DashboardChart
    {
        public string ChartType { get; set; }
        public string Title { get; set; }
        public List<string> Labels { get; set; }
        public List<DashboardDataset> Datasets { get; set; }
        public Dictionary<string, object> Options { get; set; }
    }

    public class DashboardDataset
    {
        public string Label { get; set; }
        public List<object> Data { get; set; }
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public int BorderWidth { get; set; }
        public bool Fill { get; set; }
    }
}
