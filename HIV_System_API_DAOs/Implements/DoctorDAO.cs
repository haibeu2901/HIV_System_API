using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.DoctorDTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class DoctorDAO : IDoctorDAO
    {
        private readonly HivSystemApiContext _context;
        private static DoctorDAO? _instance;


        public DoctorDAO()
        {
            _context = new HivSystemApiContext();
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

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.Acc)
                .Include(d => d.DoctorWorkSchedules)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors
                .Include(d => d.Acc)
                .Include(d => d.DoctorWorkSchedules)
                .FirstOrDefaultAsync(d => d.DctId == id);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            // Attach Acc if not tracked
            if (doctor.Acc != null)
            {
                var existingAcc = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccId == doctor.Acc.AccId);
                if (existingAcc != null)
                {
                    doctor.Acc = existingAcc;
                }
                else
                {
                    _context.Accounts.Attach(doctor.Acc);
                }
            }

            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
            // Load related Acc after save
            await _context.Entry(doctor).Reference(d => d.Acc).LoadAsync();
            return doctor;
        }

        public async Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            var existingDoctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == id);

            if (existingDoctor == null)
                return null;

            // Update Doctor fields
            existingDoctor.Degree = doctor.Degree;
            existingDoctor.Bio = doctor.Bio;

            // Update Acc if provided
            if (doctor.Acc != null)
            {
                if (existingDoctor.Acc == null || existingDoctor.Acc.AccId != doctor.Acc.AccId)
                {
                    var Acc = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == doctor.Acc.AccId);
                    if (Acc != null)
                    {
                        existingDoctor.Acc = Acc;
                        existingDoctor.AccId = Acc.AccId;
                    }
                }
                else
                {
                    existingDoctor.Acc.Email = doctor.Acc.Email;
                    existingDoctor.Acc.Fullname = doctor.Acc.Fullname;
                    existingDoctor.Acc.Dob = doctor.Acc.Dob;
                    existingDoctor.Acc.Gender = doctor.Acc.Gender;
                    existingDoctor.Acc.Roles = doctor.Acc.Roles;
                    existingDoctor.Acc.IsActive = doctor.Acc.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            await _context.Entry(existingDoctor).Reference(d => d.Acc).LoadAsync();
            return existingDoctor;
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == id);

            if (doctor == null)
                return false;

            // Optionally, handle related entities (e.g., DoctorWorkSchedules) if cascade delete is not configured
            var workSchedules = await _context.DoctorWorkSchedules
                .Where(ws => ws.DoctorId == id)
                .ToListAsync();
            if (workSchedules.Any())
            {
                _context.DoctorWorkSchedules.RemoveRange(workSchedules);
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Doctor>> GetDoctorsByDateAndTimeAsync(DateOnly appointmentDate, TimeOnly appointmentTime)
        {
            const int APPOINTMENT_DURATION_MINUTES = 30;
            const int CANCELLED_STATUS = 4;
            var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(APPOINTMENT_DURATION_MINUTES));

            // Step 1: Get all doctors with their work schedules for the specific date
            var doctorsWithSchedules = await _context.Doctors
                .Include(d => d.Acc)
                .Include(d => d.DoctorWorkSchedules.Where(ws =>
                    ws.WorkDate == appointmentDate &&
                    ws.IsAvailable == true))
                .Where(d => d.DoctorWorkSchedules.Any(ws =>
                    ws.WorkDate == appointmentDate &&
                    ws.IsAvailable == true))
                .ToListAsync();

            // Step 2: Get all existing appointments for these doctors on the specified date
            var doctorIds = doctorsWithSchedules.Select(d => d.DctId).ToList();
            var existingAppointments = await _context.Appointments
                .Where(a =>
                    doctorIds.Contains(a.DctId) &&
                    a.ApmtDate == appointmentDate &&
                    a.ApmTime.HasValue && // Only consider appointments with actual scheduled times
                    a.ApmStatus != CANCELLED_STATUS)
                .Select(a => new
                {
                    a.DctId,
                    ApmTime = a.ApmTime!.Value, // Safe to use ! since we filtered for HasValue
                    EndTime = a.ApmTime!.Value.Add(TimeSpan.FromMinutes(APPOINTMENT_DURATION_MINUTES))
                })
                .ToListAsync();

            // Step 3: Check each doctor's availability across all their time slots
            var availableDoctors = new List<Doctor>();

            foreach (var doctor in doctorsWithSchedules)
            {
                bool hasAvailableSlot = false;

                // Check each work schedule/time slot for this doctor on the specific date
                foreach (var schedule in doctor.DoctorWorkSchedules)
                {
                    // Check if this time slot can accommodate the appointment duration
                    if (schedule.StartTime <= appointmentTime && schedule.EndTime >= appointmentEndTime)
                    {
                        // Check if there are any conflicting appointments in this time slot
                        var conflictingAppointments = existingAppointments
                            .Where(a => a.DctId == doctor.DctId)
                            .Where(a =>
                                // Check for time overlap: two appointments overlap if one starts before the other ends
                                a.ApmTime < appointmentEndTime &&
                                a.EndTime > appointmentTime &&
                                // Ensure the conflicting appointment falls within this work schedule
                                a.ApmTime >= schedule.StartTime &&
                                a.ApmTime < schedule.EndTime)
                            .ToList();

                        if (!conflictingAppointments.Any())
                        {
                            hasAvailableSlot = true;
                            break; // Found an available slot, no need to check other slots
                        }
                    }
                }

                if (hasAvailableSlot)
                {
                    availableDoctors.Add(doctor);
                }
            }

            return availableDoctors;
        }
    }
}
