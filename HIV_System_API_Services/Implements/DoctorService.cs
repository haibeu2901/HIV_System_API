using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.DoctorDTO;
using HIV_System_API_DTOs.DoctorWorkScheduleDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepo _doctorRepo;
        public DoctorService()
        {
            _doctorRepo = new DoctorRepo();
        }

        private Doctor MapToEntity(DoctorRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Yêu cầu DTO bác sĩ không được để trống.");

            return new Doctor
            {
                AccId = dto.AccId,
                DctId = dto.AccId, // Set DctId to match AccId
                Degree = dto.Degree,
                Bio = dto.Bio
                // Navigation properties (Acc, Appointments, DoctorWorkSchedules) are not set here
            };
        }

        private DoctorResponseDTO MapToResponseDTO(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor), "Thực thể bác sĩ không được để trống.");

            var workSchedules = doctor.DoctorWorkSchedules?
                .Select(ws => new DoctorWorkScheduleResponseDTO
                {
                    DocWorkScheduleId = ws.DwsId,
                    DoctorId = ws.DoctorId,
                    DayOfWeek = ws.DayOfWeek ?? 0,
                    StartTime = ws.StartTime,
                    EndTime = ws.EndTime,
                    WorkDate = ws.WorkDate,
                    IsAvailable = ws.IsAvailable
                }).ToList() ?? new List<DoctorWorkScheduleResponseDTO>();

            return new DoctorResponseDTO
            {
                DoctorId = doctor.DctId,
                Degree = doctor.Degree,
                Bio = doctor.Bio,
                AccId = doctor.AccId,
                Account = doctor.Acc == null ? null : new AccountResponseDTO
                {
                    AccId = doctor.Acc.AccId,
                    Email = doctor.Acc.Email,
                    Fullname = doctor.Acc.Fullname,
                    Dob = doctor.Acc.Dob,
                    Gender = doctor.Acc.Gender,
                    Roles = doctor.Acc.Roles,
                    IsActive = doctor.Acc.IsActive
                },
                WorkSchedule = workSchedules
            };
        }

        public async Task<DoctorResponseDTO> CreateDoctorAsync(DoctorRequestDTO doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor), "Yêu cầu DTO bác sĩ không được để trống.");

            // Validation: AccId must be positive
            if (doctor.AccId <= 0)
                throw new ArgumentException("ID tài khoản không hợp lệ.", nameof(doctor.AccId));

            // Validation: Degree is required and should not be empty
            if (string.IsNullOrWhiteSpace(doctor.Degree))
                throw new ArgumentException("Bằng cấp là bắt buộc.", nameof(doctor.Degree));

            // Validation: Bio is required and should not be empty
            if (string.IsNullOrWhiteSpace(doctor.Bio))
                throw new ArgumentException("Tiểu sử là bắt buộc.", nameof(doctor.Bio));

            // Map DTO to entity
            var doctorEntity = MapToEntity(doctor);

            try
            {
                // Create doctor in repository
                var createdDoctor = await _doctorRepo.CreateDoctorAsync(doctorEntity);

                // Fetch the full doctor entity with navigation properties (e.g., Acc)
                var fullDoctor = await _doctorRepo.GetDoctorByIdAsync(createdDoctor.DctId);
                if (fullDoctor == null)
                    throw new InvalidOperationException("Không thể truy xuất thông tin bác sĩ vừa tạo.");

                // Map to response DTO
                return MapToResponseDTO(fullDoctor);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể tạo mới bác sĩ.", ex);
            }
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(id));

            try
            {
                var result = await _doctorRepo.DeleteDoctorAsync(id);
                if (!result)
                    throw new InvalidOperationException($"Không tìm thấy bác sĩ với ID {id} để xóa.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể xóa bác sĩ với ID {id}.", ex);
            }
        }

        public async Task<List<DoctorResponseDTO>> GetAllDoctorsAsync()
        {
            try
            {
                var doctors = await _doctorRepo.GetAllDoctorsAsync();
                if (doctors == null || !doctors.Any())
                    throw new InvalidOperationException("Không tìm thấy bác sĩ nào.");

                var result = new List<DoctorResponseDTO>();

                foreach (var doctor in doctors)
                {
                    // Defensive: skip if Acc is null (should not happen if DB is correct)
                    if (doctor.Acc == null)
                        continue;

                    result.Add(MapToResponseDTO(doctor));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất danh sách bác sĩ.", ex);
            }
        }

        public async Task<DoctorResponseDTO?> GetDoctorByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(id));

            try
            {
                // Fetch the doctor entity by id from the repository
                var doctor = await _doctorRepo.GetDoctorByIdAsync(id);

                // If not found or Acc navigation property is null, throw exception
                if (doctor == null || doctor.Acc == null)
                    throw new KeyNotFoundException($"Không tìm thấy bác sĩ với ID {id}.");

                // Map to response DTO
                return MapToResponseDTO(doctor);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất bác sĩ với ID {id}.", ex);
            }
        }

        public async Task<DoctorResponseDTO?> UpdateDoctorAsync(int id, DoctorRequestDTO doctor)
        {
            if (id <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(id));

            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor), "Yêu cầu DTO bác sĩ không được để trống.");

            // Validation: AccId must be positive
            if (doctor.AccId <= 0)
                throw new ArgumentException("ID tài khoản không hợp lệ.", nameof(doctor.AccId));

            // Validation: Degree is required and should not be empty
            if (string.IsNullOrWhiteSpace(doctor.Degree))
                throw new ArgumentException("Bằng cấp là bắt buộc.", nameof(doctor.Degree));

            // Validation: Bio is required and should not be empty
            if (string.IsNullOrWhiteSpace(doctor.Bio))
                throw new ArgumentException("Tiểu sử là bắt buộc.", nameof(doctor.Bio));

            try
            {
                // Fetch the existing doctor entity
                var existingDoctor = await _doctorRepo.GetDoctorByIdAsync(id);
                if (existingDoctor == null)
                    throw new KeyNotFoundException($"Không tìm thấy bác sĩ với ID {id}.");

                // Update properties
                existingDoctor.Degree = doctor.Degree;
                existingDoctor.Bio = doctor.Bio;
                existingDoctor.AccId = doctor.AccId;

                // Update in repository
                var updatedDoctor = await _doctorRepo.UpdateDoctorAsync(id, existingDoctor);
                if (updatedDoctor == null)
                    throw new InvalidOperationException($"Không thể cập nhật bác sĩ với ID {id}.");

                // Fetch the full doctor entity with navigation properties (e.g., Acc)
                var fullDoctor = await _doctorRepo.GetDoctorByIdAsync(updatedDoctor.DctId);
                if (fullDoctor == null || fullDoctor.Acc == null)
                    throw new InvalidOperationException($"Không thể truy xuất thông tin bác sĩ vừa cập nhật với ID {id}.");

                // Map to response DTO
                return MapToResponseDTO(fullDoctor);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể cập nhật bác sĩ với ID {id}.", ex);
            }
        }

        public async Task<List<DoctorResponseDTO>> GetDoctorsByDateAndTimeAsync(DateOnly apmtDate, TimeOnly apmTime)
        {
            // Validation: Ensure date is not default
            if (apmtDate == default)
                throw new ArgumentException("Ngày cuộc hẹn là bắt buộc.", nameof(apmtDate));

            // Validation: Ensure time is not default
            if (apmTime == default)
                throw new ArgumentException("Thời gian cuộc hẹn là bắt buộc.", nameof(apmTime));

            try
            {
                var doctors = await _doctorRepo.GetDoctorsByDateAndTimeAsync(apmtDate, apmTime);

                // Return empty list instead of throwing exception - this is a valid business scenario
                if (doctors == null || !doctors.Any())
                {
                    return new List<DoctorResponseDTO>();
                }

                var result = new List<DoctorResponseDTO>();

                foreach (var doctor in doctors)
                {
                    // Defensive: skip if Acc is null (should not happen if DB is correct)
                    if (doctor.Acc == null)
                    {
                        // Log this as a warning instead of silently continuing
                        // _logger.LogWarning($"Doctor with ID {doctor.DctId} has null Account. Skipping.");
                        continue;
                    }

                    result.Add(MapToResponseDTO(doctor));
                }

                return result;
            }
            catch (ArgumentException)
            {
                // Re-throw validation exceptions as they are client errors (400 Bad Request)
                throw;
            }
            catch (Exception ex)
            {
                // Log unexpected errors and throw with more specific message
                // _logger.LogError(ex, "Unexpected error occurred while retrieving doctors for date {Date} and time {Time}", apmtDate, apmTime);
                throw new InvalidOperationException($"Không thể truy xuất danh sách bác sĩ cho ngày {apmtDate} và thời gian {apmTime}.", ex);
            }
        }

        public async Task<DoctorProfileResponse?> GetDoctorProfileAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(id));

            try
            {
                var doctor = await _doctorRepo.GetDoctorByIdAsync(id);
                if (doctor == null || doctor.Acc == null)
                    throw new KeyNotFoundException($"Không tìm thấy bác sĩ với ID {id}.");

                return new DoctorProfileResponse
                {
                    Degree = doctor.Degree,
                    Bio = doctor.Bio,
                    Gender = doctor.Acc.Gender,
                    Email = doctor.Acc.Email,
                    Fullname = doctor.Acc.Fullname,
                    Dob = doctor.Acc.Dob
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất hồ sơ bác sĩ với ID {id}.", ex);
            }
        }
    }
}