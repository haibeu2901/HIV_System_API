using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IDoctorRepo
    {
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task<Doctor?> GetDoctorByIdAsync(int id);
        Task<Doctor> CreateDoctorAsync(Doctor doctor);
        Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor);
        Task<bool> DeleteDoctorAsync(int id);
    }
}
