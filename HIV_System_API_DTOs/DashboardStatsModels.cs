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
        public Dictionary<string, int> UserDistribution { get; set; } = new();
        public List<dynamic> RecentActivities { get; set; } = new();
    }

    public class DoctorDashboardStats
    {
        public int TodayAppointments { get; set; }
        public int WeeklyAppointments { get; set; }
        public int MonthlyAppointments { get; set; }
        public int TotalPatients { get; set; }
        public int UpcomingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<dynamic> TodaySchedule { get; set; } = new();
        public List<dynamic> RecentPatients { get; set; } = new();
    }

    public class PatientDashboardStats
    {
        public int UpcomingAppointments { get; set; }
        public int TotalAppointments { get; set; }
        public int RecentTestResults { get; set; }
        public int ActiveRegimens { get; set; }
        public decimal TotalPayments { get; set; }
        public dynamic? NextAppointment { get; set; }
        public List<dynamic> RecentTestResultsList { get; set; } = new();
        public List<dynamic> CurrentMedications { get; set; } = new();
    }

    public class StaffDashboardStats
    {
        public int TodayTestResults { get; set; }
        public int MonthlyTestResults { get; set; }
        public int TotalTestResults { get; set; }
        public int PendingTests { get; set; }
        public int BlogPosts { get; set; }
        public List<dynamic> RecentTestResults { get; set; } = new();
        public List<dynamic> WorkloadSummary { get; set; } = new();
    }

    public class ManagerDashboardStats
    {
        public int TotalDoctors { get; set; }
        public int TotalStaff { get; set; }
        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public int MonthlyAppointments { get; set; }
        public int PendingTests { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<dynamic> DoctorPerformance { get; set; } = new();
        public List<dynamic> StaffPerformance { get; set; } = new();
        public List<dynamic> ServiceUtilization { get; set; } = new();
    }

    public class DashboardAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ActionRequired { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
    }

    public class DashboardMetric
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double? PercentageChange { get; set; }
        public string Trend { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class DashboardChart
    {
        public string ChartType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new();
        public List<DashboardDataset> Datasets { get; set; } = new();
        public Dictionary<string, object> Options { get; set; } = new();
    }

    public class DashboardDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<object> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
        public int BorderWidth { get; set; }
        public bool Fill { get; set; }
    }
}
