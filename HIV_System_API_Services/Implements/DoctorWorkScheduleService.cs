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
                DayOfWeek = doctorWorkSchedule.DayOfWeek.HasValue ? doctorWorkSchedule.DayOfWeek.Value : 0,
                StartTime = doctorWorkSchedule.StartTime,
                EndTime = doctorWorkSchedule.EndTime
            };
        }

        public async Task<DoctorWorkScheduleResponseDTO> CreateDoctorWorkScheduleAsync(DoctorWorkScheduleRequestDTO doctorWorkSchedule)
        {
            var entity = MapToEntity(doctorWorkSchedule);
            var createdEntity = await _doctorWorkScheduleRepo.CreateDoctorWorkScheduleAsync(entity);
            return MapToResponseDTO(createdEntity);
        }

        public async Task<bool> DeleteDoctorWorkScheduleAsync(int id)
        {
            return await _doctorWorkScheduleRepo.DeleteDoctorWorkScheduleAsync(id);
        }

        public async Task<DoctorWorkScheduleResponseDTO?> GetDoctorWorkScheduleByIdAsync(int id)
        {
            var entity = await _doctorWorkScheduleRepo.GetDoctorWorkScheduleByIdAsync(id);
            if (entity == null)
                return null;
            return MapToResponseDTO(entity);
        }

        public async Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesAsync()
        {
            var entities = await _doctorWorkScheduleRepo.GetDoctorWorkSchedulesAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<DoctorWorkScheduleResponseDTO> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkScheduleRequestDTO doctorWorkSchedule)
        {
            var entity = MapToEntity(doctorWorkSchedule);
            var updatedEntity = await _doctorWorkScheduleRepo.UpdateDoctorWorkScheduleAsync(id, entity);
            return MapToResponseDTO(updatedEntity);
        }
    }
}
