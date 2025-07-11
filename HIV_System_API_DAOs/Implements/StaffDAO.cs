using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class StaffDAO : IStaffDAO
    {
        private readonly HivSystemApiContext _context;
        private static StaffDAO? _instance;


        public StaffDAO()
        {
            _context = new HivSystemApiContext();
        }
        public static StaffDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StaffDAO();
                }
                return _instance;
            }

        }

        public async Task<List<Staff>> GetAllStaffsAsync()
        {
            return await _context.Staff
                .Include(s => s.Acc)
                .ToListAsync();
        }

        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            return await _context.Staff
                .Include(s => s.Acc)
                .FirstOrDefaultAsync(s => s.StfId == id);
        }

        public async Task<Staff> CreateStaffAsync(Staff staff)
        {
            if (staff == null)
                throw new ArgumentNullException(nameof(staff));

            // Attach Acc if not tracked
            if (staff.Acc != null)
            {
                var existingAcc = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccId == staff.Acc.AccId);
                if (existingAcc != null)
                {
                    staff.Acc = existingAcc;
                }
                else
                {
                    _context.Accounts.Attach(staff.Acc);
                }
            }

            await _context.Staff.AddAsync(staff);
            await _context.SaveChangesAsync();
            // Load related Acc after save
            await _context.Entry(staff).Reference(s => s.Acc).LoadAsync();
            return staff;
        }

        public async Task<Staff?> UpdateStaffAsync(int id, Staff staff)
        {
            if (staff == null)
                throw new ArgumentNullException(nameof(staff));

            var existingStaff = await _context.Staff
                .Include(s => s.Acc)
                .FirstOrDefaultAsync(s => s.StfId == id);

            if (existingStaff == null)
                return null;

            // Update Staff fields
            existingStaff.Degree = staff.Degree;
            existingStaff.Bio = staff.Bio;

            // Update Acc if provided
            if (staff.Acc != null)
            {
                if (existingStaff.Acc == null || existingStaff.Acc.AccId != staff.Acc.AccId)
                {
                    var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == staff.Acc.AccId);
                    if (acc != null)
                    {
                        existingStaff.Acc = acc;
                        existingStaff.AccId = acc.AccId;
                    }
                }
                else
                {
                    existingStaff.Acc.Email = staff.Acc.Email;
                    existingStaff.Acc.Fullname = staff.Acc.Fullname;
                    existingStaff.Acc.Dob = staff.Acc.Dob;
                    existingStaff.Acc.Gender = staff.Acc.Gender;
                    existingStaff.Acc.Roles = staff.Acc.Roles;
                    existingStaff.Acc.IsActive = staff.Acc.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            await _context.Entry(existingStaff).Reference(s => s.Acc).LoadAsync();
            return existingStaff;
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            var staff = await _context.Staff
                .Include(s => s.Acc)
                .FirstOrDefaultAsync(s => s.StfId == id);

            if (staff == null)
                return false;

            // Handle related entities if cascade delete is not configured
            var componentTestResults = await _context.ComponentTestResults
                .Where(ctr => ctr.StfId == id)
                .ToListAsync();
            if (componentTestResults.Any())
            {
                _context.ComponentTestResults.RemoveRange(componentTestResults);
            }

            var socialBlogs = await _context.SocialBlogs
                .Where(sb => sb.StfId == id)
                .ToListAsync();
            if (socialBlogs.Any())
            {
                _context.SocialBlogs.RemoveRange(socialBlogs);
            }

            _context.Staff.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Staff>> GetStaffsBySearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllStaffsAsync();

            var searchTermLower = searchTerm.ToLower();

            return await _context.Staff
                .Include(s => s.Acc)
                .Where(s => s.Acc.Fullname.ToLower().Contains(searchTermLower) ||
                           s.Acc.Email.ToLower().Contains(searchTermLower) ||
                           (s.Degree != null && s.Degree.ToLower().Contains(searchTermLower)) ||
                           (s.Bio != null && s.Bio.ToLower().Contains(searchTermLower)))
                .ToListAsync();
        }
    }
}
