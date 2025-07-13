using HIV_System_API_BOs;
using HIV_System_API_DTOs;
using HIV_System_API_Repositories.Implements.DashboardRepo;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class DashboardService : IDashboardService
    {
        private readonly AdminDashboardRepo _adminRepo;
        private readonly DoctorDashboardRepo _doctorRepo;
        private readonly PatientDashboardRepo _patientRepo;
        private readonly StaffDashboardRepo _staffRepo;
        private readonly ManagerDashboardRepo _supervisorRepo;
        private readonly HivSystemApiContext _context;

        public DashboardService(
            AdminDashboardRepo adminRepo,
            DoctorDashboardRepo doctorRepo,
            PatientDashboardRepo patientRepo,
            StaffDashboardRepo staffRepo,
            ManagerDashboardRepo supervisorRepo,
            HivSystemApiContext context)
        {
            _adminRepo = adminRepo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
            _staffRepo = staffRepo;
            _supervisorRepo = supervisorRepo;
            _context = context;
        }

        private async Task ValidateRoleAsync(int userId, int expectedRole)
        {
            var user = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccId == userId && a.Roles == expectedRole);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User does not have the required role.");
            }
        }

        public async Task<AdminDashboardStats> GetAdminDashboardStatsAsync(int userId, DateOnly today)
        {
            await ValidateRoleAsync(userId, 1); // Admin role
            return await _adminRepo.GetAdminDashboardStatsAsync(today);
        }

        public async Task<DoctorDashboardStats> GetDoctorDashboardStatsAsync(int doctorId, DateTime today)
        {
            await ValidateRoleAsync(doctorId, 2); // Doctor role
            return await _doctorRepo.GetDoctorDashboardStatsAsync(doctorId, today);
        }

        public async Task<PatientDashboardStats> GetPatientDashboardStatsAsync(int patientId, DateTime today)
        {
            await ValidateRoleAsync(patientId, 3); // Patient role
            return await _patientRepo.GetPatientDashboardStatsAsync(patientId, today);
        }

        public async Task<StaffDashboardStats> GetStaffDashboardStatsAsync(int staffId, DateTime today)
        {
            await ValidateRoleAsync(staffId, 4); // Staff role
            return await _staffRepo.GetStaffDashboardStatsAsync(staffId, today);
        }

        public async Task<ManagerDashboardStats> GetManagerDashboardStatsAsync(int userId, DateTime today)
        {
            await ValidateRoleAsync(userId, 5); // Manager role
            return await _supervisorRepo.GetManagerDashboardStatsAsync(today);
        }

        public async Task<List<DashboardAlert>> GetDashboardAlertsAsync(int userId, string role, DateTime today)
        {
            int roleId = role.ToLower() switch
            {
                "admin" => 1,
                "doctor" => 2,
                "patient" => 3,
                "staff" => 4,
                "supervisor" => 5,
                _ => throw new ArgumentException("Invalid role specified.")
            };
            await ValidateRoleAsync(userId, roleId);
            return await _adminRepo.GetDashboardAlertsAsync(userId, today);
        }

        public async Task<DashboardChart> GetUserDistributionChartAsync(int userId)
        {
            await ValidateRoleAsync(userId, 1); // Only Admin can access
            return await _adminRepo.GetUserDistributionChartAsync();
        }

        public async Task<DashboardChart> GetManagerServiceChartAsync(int userId)
        {
            await ValidateRoleAsync(userId, 5); // Only Manager can access
            return await _supervisorRepo.GetManagerServiceChartAsync();
        }
    }
}
