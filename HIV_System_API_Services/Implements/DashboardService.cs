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
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo), "Kho lưu trữ bảng điều khiển quản trị không được để trống.");
            _doctorRepo = doctorRepo ?? throw new ArgumentNullException(nameof(doctorRepo), "Kho lưu trữ bảng điều khiển bác sĩ không được để trống.");
            _patientRepo = patientRepo ?? throw new ArgumentNullException(nameof(patientRepo), "Kho lưu trữ bảng điều khiển bệnh nhân không được để trống.");
            _staffRepo = staffRepo ?? throw new ArgumentNullException(nameof(staffRepo), "Kho lưu trữ bảng điều khiển nhân viên không được để trống.");
            _supervisorRepo = supervisorRepo ?? throw new ArgumentNullException(nameof(supervisorRepo), "Kho lưu trữ bảng điều khiển quản lý không được để trống.");
            _context = context ?? throw new ArgumentNullException(nameof(context), "Bối cảnh cơ sở dữ liệu không được để trống.");
        }

        private async Task ValidateRoleAsync(int userId, int expectedRole)
        {
            if (userId <= 0)
                throw new ArgumentException("ID người dùng không hợp lệ.", nameof(userId));

            if (expectedRole < 1 || expectedRole > 5)
                throw new ArgumentException("Vai trò không hợp lệ. Phải nằm trong khoảng từ 1 đến 5.", nameof(expectedRole));

            var user = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccId == userId && a.Roles == expectedRole);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Người dùng không có vai trò yêu cầu.");
            }
        }

        public async Task<AdminDashboardStats> GetAdminDashboardStatsAsync(int userId, DateTime today)
        {
            if (today == default)
                throw new ArgumentException("Ngày không hợp lệ.", nameof(today));

            try
            {
                await ValidateRoleAsync(userId, 1); // Admin role
                var result = await _adminRepo.GetAdminDashboardStatsAsync(today);
                if (result == null)
                    throw new InvalidOperationException("Không thể truy xuất thống kê bảng điều khiển quản trị.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất thống kê bảng điều khiển quản trị.", ex);
            }
        }

        public async Task<DoctorDashboardStats> GetDoctorDashboardStatsAsync(int doctorId, DateTime today)
        {
            if (doctorId <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(doctorId));

            if (today == default)
                throw new ArgumentException("Ngày không hợp lệ.", nameof(today));

            try
            {
                await ValidateRoleAsync(doctorId, 2); // Doctor role
                var result = await _doctorRepo.GetDoctorDashboardStatsAsync(doctorId, today);
                if (result == null)
                    throw new InvalidOperationException($"Không thể truy xuất thống kê bảng điều khiển bác sĩ cho ID {doctorId}.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất thống kê bảng điều khiển bác sĩ cho ID {doctorId}.", ex);
            }
        }

        public async Task<PatientDashboardStats> GetPatientDashboardStatsAsync(int patientId, DateTime today)
        {
            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ.", nameof(patientId));

            if (today == default)
                throw new ArgumentException("Ngày không hợp lệ.", nameof(today));

            try
            {
                await ValidateRoleAsync(patientId, 3); // Patient role
                var result = await _patientRepo.GetPatientDashboardStatsAsync(patientId, today);
                if (result == null)
                    throw new InvalidOperationException($"Không thể truy xuất thống kê bảng điều khiển bệnh nhân cho ID {patientId}.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất thống kê bảng điều khiển bệnh nhân cho ID {patientId}.", ex);
            }
        }

        public async Task<StaffDashboardStats> GetStaffDashboardStatsAsync(int staffId, DateTime today)
        {
            if (staffId <= 0)
                throw new ArgumentException("ID nhân viên không hợp lệ.", nameof(staffId));

            if (today == default)
                throw new ArgumentException("Ngày không hợp lệ.", nameof(today));

            try
            {
                await ValidateRoleAsync(staffId, 4); // Staff role
                var result = await _staffRepo.GetStaffDashboardStatsAsync(staffId, today);
                if (result == null)
                    throw new InvalidOperationException($"Không thể truy xuất thống kê bảng điều khiển nhân viên cho ID {staffId}.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất thống kê bảng điều khiển nhân viên cho ID {staffId}.", ex);
            }
        }

        public async Task<ManagerDashboardStats> GetManagerDashboardStatsAsync(int userId, DateTime today)
        {
            if (userId <= 0)
                throw new ArgumentException("ID người dùng không hợp lệ.", nameof(userId));

            if (today == default)
                throw new ArgumentException("Ngày không hợp lệ.", nameof(today));

            try
            {
                await ValidateRoleAsync(userId, 5); // Manager role
                var result = await _supervisorRepo.GetManagerDashboardStatsAsync(today);
                if (result == null)
                    throw new InvalidOperationException("Không thể truy xuất thống kê bảng điều khiển quản lý.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất thống kê bảng điều khiển quản lý.", ex);
            }
        }

        /* Bỏ qua phần comment vì không được sử dụng
        public async Task<List<DashboardAlert>> GetDashboardAlertsAsync(int userId, string role, DateTime today)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Vai trò không được để trống.", nameof(role));

            if (today == default)
                throw new ArgumentException("Ngày không hợp lệ.", nameof(today));

            int roleId = role.ToLower() switch
            {
                "admin" => 1,
                "doctor" => 2,
                "patient" => 3,
                "staff" => 4,
                "manager" => 5,
                _ => throw new ArgumentException("Vai trò được chỉ định không hợp lệ.", nameof(role))
            };

            try
            {
                await ValidateRoleAsync(userId, roleId);
                var result = await _adminRepo.GetDashboardAlertsAsync(userId, today);
                if (result == null || !result.Any())
                    throw new InvalidOperationException($"Không tìm thấy cảnh báo nào cho người dùng với ID {userId} và vai trò {role}.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất cảnh báo cho người dùng với ID {userId} và vai trò {role}.", ex);
            }
        }
        */

        public async Task<DashboardChart> GetUserDistributionChartAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID người dùng không hợp lệ.", nameof(userId));

            try
            {
                await ValidateRoleAsync(userId, 1); // Only Admin can access
                var result = await _adminRepo.GetUserDistributionChartAsync();
                if (result == null)
                    throw new InvalidOperationException("Không thể truy xuất biểu đồ phân bố người dùng.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất biểu đồ phân bố người dùng.", ex);
            }
        }

        public async Task<DashboardChart> GetManagerServiceChartAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID người dùng không hợp lệ.", nameof(userId));

            try
            {
                await ValidateRoleAsync(userId, 5); // Only Manager can access
                var result = await _supervisorRepo.GetManagerServiceChartAsync();
                if (result == null)
                    throw new InvalidOperationException("Không thể truy xuất biểu đồ dịch vụ quản lý.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất biểu đồ dịch vụ quản lý.", ex);
            }
        }
    }
}