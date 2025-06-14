using HIV_System_API_BOs;
using HIV_System_API_DTOs.DoctorDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorResponseDTO>> GetAllDoctorsAsync();
        Task<DoctorResponseDTO?> GetDoctorByIdAsync(int id);
        Task<DoctorResponseDTO> CreateDoctorAsync(DoctorRequestDTO doctor);
        Task<DoctorResponseDTO?> UpdateDoctorAsync(int id, DoctorRequestDTO doctor);
        Task<bool> DeleteDoctorAsync(int id);
        Task<List<DoctorResponseDTO>> GetDoctorsByDateAndTimeAsync(DateOnly apmtDate, TimeOnly apmTime);
    }
}
