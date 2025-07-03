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
            _doctorWorkScheduleRepo= new DoctorWorkScheduleRepo();
        }

        private DoctorWorkSchedule MapToEntity(DoctorWorkScheduleRequestDTO requestDTO)
        {
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
                throw new ArgumentNullException(nameof(doctorWorkSchedule));
            if (doctorWorkSchedule.DoctorId <= 0)
                throw new ArgumentException("DoctorId must be a positive integer.", nameof(doctorWorkSchedule.DoctorId));
            if (doctorWorkSchedule.DayOfWeek < 1 || doctorWorkSchedule.DayOfWeek > 7)
                throw new ArgumentOutOfRangeException(nameof(doctorWorkSchedule.DayOfWeek), "DayOfWeek must be between 1 (Sunday) and 7 (Saturday).");
            if (doctorWorkSchedule.StartTime >= doctorWorkSchedule.EndTime)
                throw new ArgumentException("StartTime must be earlier than EndTime.");
            if (doctorWorkSchedule.WorkDate < DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("WorkDate cannot be in the past.", nameof(doctorWorkSchedule.WorkDate));

            try
            {
                var entity = MapToEntity(doctorWorkSchedule);
                var createdEntity = await _doctorWorkScheduleRepo.CreateDoctorWorkScheduleAsync(entity);
                return MapToResponseDTO(createdEntity);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to create doctor work schedule: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while creating doctor work schedule.", ex.InnerException);
            }
        }

        public async Task<bool> DeleteDoctorWorkScheduleAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be a positive integer.", nameof(id));

            try
            {
                var result = await _doctorWorkScheduleRepo.DeleteDoctorWorkScheduleAsync(id);
                if (!result)
                    throw new KeyNotFoundException($"Doctor work schedule with ID {id} not found.");

                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to delete doctor work schedule with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred while deleting doctor work schedule with ID {id}.", ex.InnerException);
            }
        }

        public async Task<DoctorWorkScheduleResponseDTO?> GetDoctorWorkScheduleByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be a positive integer.", nameof(id));

            try
            {
                var entity = await _doctorWorkScheduleRepo.GetDoctorWorkScheduleByIdAsync(id);
                if (entity == null)
                    return null;

                return MapToResponseDTO(entity);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve doctor work schedule with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred while retrieving doctor work schedule with ID {id}.", ex.InnerException);
            }
        }

        public async Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesAsync()
        {
            try
            {
                var entities = await _doctorWorkScheduleRepo.GetDoctorWorkSchedulesAsync();
                if (entities == null || !entities.Any())
                    return new List<DoctorWorkScheduleResponseDTO>();

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve doctor work schedules: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while retrieving doctor work schedules.", ex.InnerException);
            }
        }

        public async Task<DoctorWorkScheduleResponseDTO> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkScheduleRequestDTO doctorWorkSchedule)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be a positive integer.", nameof(id));
            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule));
            if (doctorWorkSchedule.DoctorId <= 0)
                throw new ArgumentException("DoctorId must be a positive integer.", nameof(doctorWorkSchedule.DoctorId));
            if (doctorWorkSchedule.DayOfWeek < 1 || doctorWorkSchedule.DayOfWeek > 7)
                throw new ArgumentOutOfRangeException(nameof(doctorWorkSchedule.DayOfWeek), "DayOfWeek must be between 1 (Sunday) and 7 (Saturday).");
            if (doctorWorkSchedule.StartTime >= doctorWorkSchedule.EndTime)
                throw new ArgumentException("StartTime must be earlier than EndTime.");
            if (doctorWorkSchedule.WorkDate < DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("WorkDate cannot be in the past.", nameof(doctorWorkSchedule.WorkDate));

            try
            {
                var entity = MapToEntity(doctorWorkSchedule);
                var updatedEntity = await _doctorWorkScheduleRepo.UpdateDoctorWorkScheduleAsync(id, entity);
                return MapToResponseDTO(updatedEntity);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException($"Doctor work schedule with ID {id} not found.", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to update doctor work schedule with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred while updating doctor work schedule with ID {id}.", ex.InnerException);
            }
        }

        public async Task<List<PersonalWorkScheduleResponseDTO>> GetPersonalWorkSchedulesAsync(int doctorId)
        {
            if (doctorId <= 0)
                throw new ArgumentException("DoctorId must be a positive integer.", nameof(doctorId));

            try
            {
                var schedules = await _doctorWorkScheduleRepo.GetPersonalWorkSchedulesAsync(doctorId);
                if (schedules == null || !schedules.Any())
                    return new List<PersonalWorkScheduleResponseDTO>();

                var result = new List<PersonalWorkScheduleResponseDTO>();
                foreach (var schedule in schedules)
                {
                    if (!schedule.DayOfWeek.HasValue)
                        continue;

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
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve work schedules for doctor with ID {doctorId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred while retrieving work schedules for doctor with ID {doctorId}.", ex.InnerException);
            }
        }

        public async Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesByDoctorIdAsync(int doctorId)
        {
            if (doctorId <= 0)
                throw new ArgumentException("DoctorId must be a positive integer.", nameof(doctorId));

            try
            {
                var entities = await _doctorWorkScheduleRepo.GetDoctorWorkSchedulesByDoctorIdAsync(doctorId);
                if (entities == null || !entities.Any())
                    return new List<DoctorWorkScheduleResponseDTO>();

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve work schedules for doctor with ID {doctorId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred while retrieving work schedules for doctor with ID {doctorId}.", ex.InnerException);
            }
        }
    }
}
