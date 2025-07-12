using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IStaffRepo
    {
        Task<List<Staff>> GetAllStaffsAsync();
        Task<Staff?> GetStaffByIdAsync(int id);
        Task<Staff> CreateStaffAsync(Staff staff);
        Task<Staff?> UpdateStaffAsync(int id, Staff staff);
        Task<bool> DeleteStaffAsync(int id);
        Task<List<Staff>> GetStaffsBySearchAsync(string searchTerm);
    }
}
