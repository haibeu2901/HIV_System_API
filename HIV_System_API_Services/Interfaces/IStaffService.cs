using HIV_System_API_BOs;
using HIV_System_API_DTOs.StaffDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IStaffService
    {
        Task<List<StaffResponseDTO>> GetAllStaffsAsync();
        Task<StaffResponseDTO?> GetStaffByIdAsync(int id);
        Task<StaffResponseDTO> CreateStaffAsync(StaffRequestDTO staffDto);
        Task<StaffResponseDTO?> UpdateStaffAsync(int id, StaffRequestDTO staffDto);
        Task<bool> DeleteStaffAsync(int id);
        Task<List<StaffResponseDTO>> GetStaffsBySearchAsync(string searchTerm);
    }
}
