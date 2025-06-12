using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.DoctorDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class DoctorDAO : IDoctorDAO
    {
        private readonly HivSystemContext _context;
        private static DoctorDAO _instance;


        public DoctorDAO()
        {
            _context = new HivSystemContext();
        }
        public static DoctorDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DoctorDAO();
                }
                return _instance;
            }

        }

        public async Task<DoctorResponseDTO> CreateDoctorAsync(DoctorRequestDTO doctorRequest)
        {
            if (doctorRequest == null)
                throw new ArgumentNullException(nameof(doctorRequest));

            // Find the related Account
            var account = await _context.Accounts.FindAsync(doctorRequest.AccId);
            if (account == null)
                throw new InvalidOperationException($"Account with ID {doctorRequest.AccId} not found.");

            // Create Doctor entity
            var doctor = new Doctor
            {
                AccId = doctorRequest.AccId,
                Degree = doctorRequest.Degree,
                Bio = doctorRequest.Bio
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Prepare response DTO
            var response = new DoctorResponseDTO
            {
                DctId = doctor.DctId,
                AccId = doctor.AccId,
                Degree = doctor.Degree,
                Bio = doctor.Bio,
                Account = new AccountResponseDTO
                {
                    AccId = account.AccId,
                    AccUsername = account.AccUsername,
                    AccPassword = account.AccPassword,
                    Email = account.Email,
                    Fullname = account.Fullname,
                    Dob = account.Dob,
                    Gender = account.Gender,
                    Roles = account.Roles,
                    IsActive = account.IsActive
                }
            };

            return response;
        }

        public Task<bool> DeleteDoctorAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<DoctorResponseDTO>> GetAllDoctorsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DoctorResponseDTO?> GetDoctorByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<DoctorResponseDTO?> UpdateDoctorAsync(int id, DoctorRequestDTO doctorRequest)
        {
            throw new NotImplementedException();
        }
    }
}
