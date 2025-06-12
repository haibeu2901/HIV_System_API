using HIV_System_API_DTOs.DoctorDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IDoctorDAO
    {
        Task<List<DoctorResponseDTO>> GetAllDoctorsAsync();
        Task<DoctorResponseDTO?> GetDoctorByIdAsync(int id);
        Task<DoctorResponseDTO> CreateDoctorAsync(DoctorRequestDTO doctorRequest);
        Task<DoctorResponseDTO?> UpdateDoctorAsync(int id, DoctorRequestDTO doctorRequest);
        Task<bool> DeleteDoctorAsync(int id);
    }
}
