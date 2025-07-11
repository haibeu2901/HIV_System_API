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
    public class StaffRepo : IStaffRepo
    {
        public async Task<Staff> CreateStaffAsync(Staff staff)
        {
            return await StaffDAO.Instance.CreateStaffAsync(staff);
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            return await StaffDAO.Instance.DeleteStaffAsync(id);
        }

        public async Task<List<Staff>> GetAllStaffsAsync()
        {
            return await StaffDAO.Instance.GetAllStaffsAsync();
        }

        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            return await StaffDAO.Instance.GetStaffByIdAsync(id);
        }

        public async Task<List<Staff>> GetStaffsBySearchAsync(string searchTerm)
        {
            return await StaffDAO.Instance.GetStaffsBySearchAsync(searchTerm);
        }

        public async Task<Staff?> UpdateStaffAsync(int id, Staff staff)
        {
            return await StaffDAO.Instance.UpdateStaffAsync(id, staff);
        }
    }
}
