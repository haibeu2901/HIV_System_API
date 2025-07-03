using HIV_System_API_BOs;
using HIV_System_API_DTOs.DoctorWorkScheduleDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IDoctorWorkScheduleService
    {
        Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesAsync();
        Task<DoctorWorkScheduleResponseDTO?> GetDoctorWorkScheduleByIdAsync(int id);
        Task<DoctorWorkScheduleResponseDTO> CreateDoctorWorkScheduleAsync(DoctorWorkScheduleRequestDTO doctorWorkSchedule);
        Task<bool> DeleteDoctorWorkScheduleAsync(int id);
        Task<DoctorWorkScheduleResponseDTO> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkScheduleRequestDTO doctorWorkSchedule);
        Task<List<PersonalWorkScheduleResponseDTO>> GetPersonalWorkSchedulesAsync(int doctorId);
        Task<List<DoctorWorkScheduleResponseDTO>> GetDoctorWorkSchedulesByDoctorIdAsync(int doctorId);
    }
}
