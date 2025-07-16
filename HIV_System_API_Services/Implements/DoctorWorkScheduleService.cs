using HIV_System_API_BOs;
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
    public class DoctorWorkScheduleService : IDoctorWorkScheduleService
    {
        private readonly IDoctorWorkScheduleRepo _doctorWorkScheduleRepo;

        public DoctorWorkScheduleService()
        {
            _doctorWorkScheduleRepo = new DoctorWorkScheduleRepo();
        }

        private DoctorWorkSchedule MapToEntity(DoctorWorkScheduleRequestDTO requestDTO)
        {
            if (requestDTO == null)
                throw new ArgumentNullException(nameof(requestDTO), "Yêu cầu DTO lịch làm việc bác sĩ không được để trống.");

            return new DoctorWorkSchedule
            {
                DoctorId = requestDTO.DoctorId,
                DayOfWeek = (byte)requestDTO.DayOfWeek,
                WorkDate = requestDTO.WorkDate,
                IsAvailable = requestDTO.IsAvailable,
                StartTime = requestDTO.StartTime,
                EndTime = requestDTO.EndTime
            };
        }

        private DoctorWorkScheduleResponseDTO MapToResponseDTO(DoctorWorkSchedule doctorWorkSchedule)
        {
            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule), "Thực thể lịch làm việc bác sĩ không được để trống.");

            return new DoctorWorkScheduleResponseDTO
            {
                DocWorkScheduleId = doctorWorkSchedule.DwsId,
                DoctorId = doctorWorkSchedule.DoctorId,
                DayOfWeek = doctorWorkSchedule.DayOfWeek ?? 0,
                WorkDate = doctorWorkSchedule.WorkDate,
                IsAvailable = doctorWorkSchedule.IsAvailable,
                StartTime = doctorWorkSchedule.StartTime,
                EndTime = doctorWorkSchedule.EndTime
            };
        }

        public async Task<DoctorWorkScheduleResponseDTO> CreateDoctorWorkScheduleAsync(DoctorWorkScheduleRequestDTO doctorWorkSchedule)
        {
            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule), "Yêu cầu DTO lịch làm việc bác sĩ không được để trống.");

            if (doctorWorkSchedule.DoctorId <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(doctorWorkSchedule.DoctorId));

            if (doctorWorkSchedule.DayOfWeek < 1 || doctorWorkSchedule.DayOfWeek > 7)
                throw new ArgumentOutOfRangeException(nameof(doctorWorkSchedule.DayOfWeek), "Ngày trong tuần phải nằm trong khoảng từ 1 (Chủ Nhật) đến 7 (Thứ Bảy).");

            if (doctorWorkSchedule.StartTime >= doctorWorkSchedule.EndTime)
                throw new ArgumentException("Thời gian bắt đầu phải sớm hơn thời gian kết thúc.", nameof(doctorWorkSchedule.StartTime));

            if (doctorWorkSchedule.WorkDate < DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Ngày làm việc không được là ngày trong quá khứ.", nameof(doctorWorkSchedule.WorkDate));

            try
            {
                var entity = MapToEntity(doctorWorkSchedule);
                var createdEntity = await _doctorWorkScheduleRepo.CreateDoctorWorkScheduleAsync(entity);
                if (createdEntity == null)
                    throw new InvalidOperationException("Không thể tạo lịch làm việc bác sĩ.");

                return MapToResponseDTO(createdEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Đã xảy ra lỗi khi tạo lịch làm việc bác sĩ.", ex);
            }
        }

        public async Task<bool> DeleteDoctorWorkScheduleAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID lịch làm việc không hợp lệ.", nameof(id));

            try
            {
                var result = await _doctorWorkScheduleRepo.DeleteDoctorWorkScheduleAsync(id);
                if (!result)
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc bác sĩ với ID {id}.");

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể xóa lịch làm việc bác sĩ với ID {id}.", ex);
            }
        }

        public async Task<DoctorWorkScheduleResponseDTO?> GetDoctorWorkScheduleByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID lịch làm việc không hợp lệ.", nameof(id));

            try
            {
                var entity = await _doctorWorkScheduleRepo.GetDoctorWorkScheduleByIdAsync(id);
                if (entity == null)
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc bác sĩ với ID {id}.");

                return MapToResponseDTO(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất lịch làm việc bác sĩ với ID {id}.", ex);
            }
        }

        public async Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesAsync()
        {
            try
            {
                var entities = await _doctorWorkScheduleRepo.GetDoctorWorkSchedulesAsync();
                if (entities == null || !entities.Any())
                    throw new InvalidOperationException("Không tìm thấy lịch làm việc bác sĩ nào.");

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất danh sách lịch làm việc bác sĩ.", ex);
            }
        }

        public async Task<DoctorWorkScheduleResponseDTO> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkScheduleRequestDTO doctorWorkSchedule)
        {
            if (id <= 0)
                throw new ArgumentException("ID lịch làm việc không hợp lệ.", nameof(id));

            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule), "Yêu cầu DTO lịch làm việc bác sĩ không được để trống.");

            if (doctorWorkSchedule.DoctorId <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(doctorWorkSchedule.DoctorId));

            if (doctorWorkSchedule.DayOfWeek < 1 || doctorWorkSchedule.DayOfWeek > 7)
                throw new ArgumentOutOfRangeException(nameof(doctorWorkSchedule.DayOfWeek), "Ngày trong tuần phải nằm trong khoảng từ 1 (Chủ Nhật) đến 7 (Thứ Bảy).");

            if (doctorWorkSchedule.StartTime >= doctorWorkSchedule.EndTime)
                throw new ArgumentException("Thời gian bắt đầu phải sớm hơn thời gian kết thúc.", nameof(doctorWorkSchedule.StartTime));

            if (doctorWorkSchedule.WorkDate < DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Ngày làm việc không được là ngày trong quá khứ.", nameof(doctorWorkSchedule.WorkDate));

            try
            {
                var entity = MapToEntity(doctorWorkSchedule);
                var updatedEntity = await _doctorWorkScheduleRepo.UpdateDoctorWorkScheduleAsync(id, entity);
                if (updatedEntity == null)
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc bác sĩ với ID {id}.");

                return MapToResponseDTO(updatedEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể cập nhật lịch làm việc bác sĩ với ID {id}.", ex);
            }
        }

        public async Task<List<PersonalWorkScheduleResponseDTO>> GetPersonalWorkSchedulesAsync(int doctorId)
        {
            if (doctorId <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(doctorId));

            try
            {
                var schedules = await _doctorWorkScheduleRepo.GetPersonalWorkSchedulesAsync(doctorId);
                if (schedules == null || !schedules.Any())
                    throw new InvalidOperationException($"Không tìm thấy lịch làm việc cá nhân cho bác sĩ với ID {doctorId}.");

                var result = new List<PersonalWorkScheduleResponseDTO>();
                foreach (var schedule in schedules)
                {
                    if (!schedule.DayOfWeek.HasValue)
                        throw new InvalidOperationException("Ngày trong tuần không được để trống trong lịch làm việc cá nhân.");

                    result.Add(new PersonalWorkScheduleResponseDTO
                    {
                        DayOfWeek = schedule.DayOfWeek.Value,
                        WorkDate = schedule.WorkDate,
                        IsAvailable = schedule.IsAvailable,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất lịch làm việc cá nhân cho bác sĩ với ID {doctorId}.", ex);
            }
        }

        public async Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesByDoctorIdAsync(int doctorId)
        {
            if (doctorId <= 0)
                throw new ArgumentException("ID bác sĩ không hợp lệ.", nameof(doctorId));

            try
            {
                var entities = await _doctorWorkScheduleRepo.GetDoctorWorkSchedulesByDoctorIdAsync(doctorId);
                if (entities == null || !entities.Any())
                    throw new InvalidOperationException($"Không tìm thấy lịch làm việc cho bác sĩ với ID {doctorId}.");

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất lịch làm việc cho bác sĩ với ID {doctorId}.", ex);
            }
        }
    }
}