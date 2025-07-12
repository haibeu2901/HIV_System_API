using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.StaffDTO;
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
    public class StaffService : IStaffService
    {
        private readonly IStaffRepo _staffRepo;

        public StaffService()
        {
            _staffRepo = new StaffRepo();
        }

        private Staff MapToEntity(StaffRequestDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Staff
            {
                AccId = dto.AccId,
                StfId = dto.AccId, // Set StfId to match AccId
                Degree = dto.Degree,
                Bio = dto.Bio
                // Navigation properties (Acc, ComponentTestResults, SocialBlogs) are not set here
            };
        }

        private StaffResponseDTO MapToResponseDTO(Staff staff)
        {
            if (staff == null) throw new ArgumentNullException(nameof(staff));

            return new StaffResponseDTO
            {
                StaffId = staff.StfId,
                Degree = staff.Degree,
                Bio = staff.Bio,
                AccId = staff.AccId,
                Account = staff.Acc == null ? null : new AccountResponseDTO
                {
                    AccId = staff.Acc.AccId,
                    Email = staff.Acc.Email,
                    Fullname = staff.Acc.Fullname,
                    Dob = staff.Acc.Dob,
                    Gender = staff.Acc.Gender,
                    Roles = staff.Acc.Roles,
                    IsActive = staff.Acc.IsActive
                }
            };
        }

        public async Task<StaffResponseDTO> CreateStaffAsync(StaffRequestDTO staffDto)
        {
            if (staffDto == null)
                throw new ArgumentNullException(nameof(staffDto));

            // Validation: AccId must be positive
            if (staffDto.AccId <= 0)
                throw new ArgumentException("Account ID must be a positive integer.", nameof(staffDto.AccId));

            // Validation: Degree is required and should not be empty
            if (string.IsNullOrWhiteSpace(staffDto.Degree))
                throw new ArgumentException("Degree is required.", nameof(staffDto.Degree));

            // Validation: Bio is required and should not be empty
            if (string.IsNullOrWhiteSpace(staffDto.Bio))
                throw new ArgumentException("Bio is required.", nameof(staffDto.Bio));

            // Map DTO to entity
            var staffEntity = MapToEntity(staffDto);

            // Create staff in repository
            var createdStaff = await _staffRepo.CreateStaffAsync(staffEntity);

            // Fetch the full staff entity with navigation properties (e.g., Acc)
            var fullStaff = await _staffRepo.GetStaffByIdAsync(createdStaff.StfId);
            if (fullStaff == null)
                throw new InvalidOperationException("Failed to retrieve the created staff.");

            // Map to response DTO
            return MapToResponseDTO(fullStaff);
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            // Validation: Id must be positive
            if (id <= 0)
                throw new ArgumentException("Staff ID must be a positive integer.", nameof(id));

            // Call the repository to delete the staff by id
            return await _staffRepo.DeleteStaffAsync(id);
        }

        public async Task<List<StaffResponseDTO>> GetAllStaffsAsync()
        {
            var staffs = await _staffRepo.GetAllStaffsAsync();
            var result = new List<StaffResponseDTO>();

            foreach (var staff in staffs)
            {
                // Defensive: skip if Acc is null (should not happen if DB is correct)
                if (staff.Acc == null)
                    continue;

                result.Add(MapToResponseDTO(staff));
            }

            return result;
        }

        public async Task<StaffResponseDTO?> GetStaffByIdAsync(int id)
        {
            // Validation: Id must be positive
            if (id <= 0)
                throw new ArgumentException("Staff ID must be a positive integer.", nameof(id));

            // Fetch the staff entity by id from the repository
            var staff = await _staffRepo.GetStaffByIdAsync(id);

            // If not found or Acc navigation property is null, return null
            if (staff == null || staff.Acc == null)
                return null;

            // Map to response DTO
            return MapToResponseDTO(staff);
        }

        public async Task<List<StaffResponseDTO>> GetStaffsBySearchAsync(string searchTerm)
        {
            // Validation: Search term should not be null (empty string is allowed for getting all records)
            if (searchTerm == null)
                throw new ArgumentNullException(nameof(searchTerm));

            var staffs = await _staffRepo.GetStaffsBySearchAsync(searchTerm);
            var result = new List<StaffResponseDTO>();

            foreach (var staff in staffs)
            {
                // Defensive: skip if Acc is null (should not happen if DB is correct)
                if (staff.Acc == null)
                    continue;

                result.Add(MapToResponseDTO(staff));
            }

            return result;
        }

        public async Task<StaffResponseDTO?> UpdateStaffAsync(int id, StaffRequestDTO staffDto)
        {
            if (staffDto == null)
                throw new ArgumentNullException(nameof(staffDto));

            // Validation: Id must be positive
            if (id <= 0)
                throw new ArgumentException("Staff ID must be a positive integer.", nameof(id));

            // Validation: AccId must be positive
            if (staffDto.AccId <= 0)
                throw new ArgumentException("Account ID must be a positive integer.", nameof(staffDto.AccId));

            // Validation: Degree is required and should not be empty
            if (string.IsNullOrWhiteSpace(staffDto.Degree))
                throw new ArgumentException("Degree is required.", nameof(staffDto.Degree));

            // Validation: Bio is required and should not be empty
            if (string.IsNullOrWhiteSpace(staffDto.Bio))
                throw new ArgumentException("Bio is required.", nameof(staffDto.Bio));

            // Fetch the existing staff entity
            var existingStaff = await _staffRepo.GetStaffByIdAsync(id);
            if (existingStaff == null)
                return null;

            // Update properties
            existingStaff.Degree = staffDto.Degree;
            existingStaff.Bio = staffDto.Bio;
            existingStaff.AccId = staffDto.AccId;

            // Update in repository
            var updatedStaff = await _staffRepo.UpdateStaffAsync(id, existingStaff);
            if (updatedStaff == null)
                return null;

            // Fetch the full staff entity with navigation properties (e.g., Acc)
            var fullStaff = await _staffRepo.GetStaffByIdAsync(updatedStaff.StfId);
            if (fullStaff == null || fullStaff.Acc == null)
                return null;

            // Map to response DTO
            return MapToResponseDTO(fullStaff);
        }
    }
}
