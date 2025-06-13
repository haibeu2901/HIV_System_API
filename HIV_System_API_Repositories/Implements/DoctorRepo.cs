using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class DoctorRepo : IDoctorRepo
    {
        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            return await DoctorDAO.Instance.CreateDoctorAsync(doctor);
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            return await DoctorDAO.Instance.DeleteDoctorAsync(id);
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await DoctorDAO.Instance.GetAllDoctorsAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await DoctorDAO.Instance.GetDoctorByIdAsync(id);
        }

        public async Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor)
        {
            return await DoctorDAO.Instance.UpdateDoctorAsync(id, doctor);
        }
    }
}
