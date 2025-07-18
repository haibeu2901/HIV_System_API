using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using HIV_System_API_DTOs.ComponentTestResultDTO;
using HIV_System_API_DTOs.PatientArvMedicationDTO;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using HIV_System_API_DTOs.PaymentDTO;
using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PatientMedicalRecordService : IPatientMedicalRecordService
    {
        private readonly IPatientMedicalRecordRepo _patientMedicalRecordRepo;
        private readonly HivSystemApiContext _context;

        public PatientMedicalRecordService()
        {
            _patientMedicalRecordRepo = new PatientMedicalRecordRepo();
            _context = new HivSystemApiContext();
        }

        private PatientMedicalRecord MapToEntity(PatientMedicalRecordRequestDTO requestDTO)
        {
            return new PatientMedicalRecord
            {
                PtnId = requestDTO.PtnId
                // Collections (Appointments, PatientArvRegimen, TestResults) and navigation properties (Ptn) are not set here
                // as they are typically managed by the ORM or set elsewhere
            };
        }

        private PatientMedicalRecordResponseDTO MapToResponseDTO(PatientMedicalRecord record)
        {
            // Get all appointments for the patient - SORTED BY LATEST DATE FIRST
            var appointments = _context.Appointments
                .Where(a => a.PtnId == record.PtnId)
                .OrderByDescending(a => a.ApmtDate)
                .ThenByDescending(a => a.ApmTime)
                .Select(a => new AppointmentResponseDTO
                {
                    AppointmentId = a.ApmId,
                    PatientId = a.PtnId,
                    PatientName = a.Ptn.Acc.Fullname,
                    DoctorId = a.DctId,
                    DoctorName = a.Dct.Acc.Fullname,
                    ApmtDate = a.ApmtDate,
                    ApmTime = a.ApmTime,
                    Notes = a.Notes,
                    ApmStatus = a.ApmStatus
                })
                .ToList();

            // Get all test results for the patient medical record - SORTED BY LATEST DATE FIRST
            var testResults = _context.TestResults
                .Where(tr => tr.PmrId == record.PmrId)
                .OrderByDescending(tr => tr.TestDate)
                .Select(tr => new TestResultResponseDTO
                {
                    TestResultId = tr.TrsId,
                    PatientMedicalRecordId = tr.PmrId,
                    TestDate = tr.TestDate,
                    Result = tr.ResultValue,
                    Notes = tr.Notes,
                    ComponentTestResults = tr.ComponentTestResults
                        .Select(ctr => new ComponentTestResultResponseDTO
                        {
                            ComponentTestResultId = ctr.CtrId,
                            TestResultId = ctr.TrsId,
                            ComponentTestResultName = ctr.CtrName,
                            CtrDescription = ctr.CtrDescription,
                            ResultValue = ctr.ResultValue,
                            Notes = ctr.Notes,
                            StaffId = ctr.StfId
                        })
                        .ToList()
                })
                .ToList();

            // Get all ARV regimens for the patient medical record - SORTED BY LATEST CREATED DATE FIRST
            var arvRegimens = _context.PatientArvRegimen
                .Where(par => par.PmrId == record.PmrId)
                .OrderByDescending(par => par.CreatedAt)
                .Select(par => new PatientArvRegimenResponseDTO
                {
                    PatientArvRegiId = par.ParId,
                    PatientMedRecordId = par.PmrId,
                    Notes = par.Notes,
                    RegimenLevel = par.RegimenLevel,
                    CreatedAt = par.CreatedAt,
                    StartDate = par.StartDate,
                    EndDate = par.EndDate,
                    RegimenStatus = par.RegimenStatus,
                    TotalCost = par.TotalCost,
                    ARVMedications = par.PatientArvMedications
                        .Select(pam => new PatientArvMedicationResponseDTO
                        {
                            PatientArvMedId = pam.PamId,
                            PatientArvRegiId = pam.ParId,
                            ArvMedId = pam.AmdId,
                            Quantity = pam.Quantity,
                            MedicationDetail = new ArvMedicationDetailResponseDTO
                            {
                                ARVMedicationName = pam.Amd.MedName,
                                ARVMedicationDescription = pam.Amd.MedDescription,
                                ARVMedicationDosage = pam.Amd.Dosage,
                                ARVMedicationPrice = pam.Amd.Price,
                                ARVMedicationManufacturer = pam.Amd.Manufactorer
                            }
                        })
                        .ToList()
                })
                .ToList();

            var Payments = _context.Payments
                .Where(p => p.PmrId == record.PmrId)
                .Select(p => new PaymentResponseDTO
                {
                    PayId = p.PayId,
                    PmrId = p.PmrId,
                    SrvId = p.SrvId ?? null,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = p.PaymentMethod ?? "card",
                    Description = p.Notes,
                    PaymentStatus = 0, // Pending
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PaymentDate = DateTime.UtcNow,
                    PaymentIntentId = p.PaymentIntentId,
                    PatientId = p.Pmr.PtnId,
                    PatientName = p.Pmr.Ptn.Acc.Fullname,
                    PatientEmail = p.Pmr.Ptn.Acc.Email,
                    ServiceName = p.Srv != null ? p.Srv.ServiceName : null,
                    ServicePrice = p.Srv != null ? p.Srv.Price : null
                })
                .ToList();

            return new PatientMedicalRecordResponseDTO
            {
                PmrId = record.PmrId,
                PtnId = record.PtnId,
                Appointments = appointments,
                TestResults = testResults,
                ARVRegimens = arvRegimens,
                Payments = Payments,
            };
        }

        public async Task<PatientMedicalRecordResponseDTO> CreatePatientMedicalRecordAsync(PatientMedicalRecordRequestDTO record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            // Validation: PtnId must be positive
            if (record.PtnId <= 0)
                throw new ArgumentException("ID bệnh nhân (PtnId) phải là số dương.", nameof(record.PtnId));

            // Optionally, check if a record for this patient already exists (if business logic requires)
            // var existing = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(record.PtnId);
            // if (existing != null)
            //     throw new InvalidOperationException($"A medical record for patient ID {record.PtnId} already exists.");

            var entity = MapToEntity(record);
            var createdEntity = await _patientMedicalRecordRepo.CreatePatientMedicalRecordAsync(entity);
            return MapToResponseDTO(createdEntity);
        }

        public async Task<bool> DeletePatientMedicalRecordAsync(int id)
        {
            return await _patientMedicalRecordRepo.DeletePatientMedicalRecordAsync(id);
        }

        public async Task<List<PatientMedicalRecordResponseDTO>> GetAllPatientMedicalRecordsAsync()
        {
            var records = await _patientMedicalRecordRepo.GetAllPatientMedicalRecordsAsync();
            return records.Select(MapToResponseDTO).ToList();
        }

        public async Task<PatientMedicalRecordResponseDTO?> GetPatientMedicalRecordByIdAsync(int id)
        {
            var record = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(id);
            if (record == null)
                return null;
            return MapToResponseDTO(record);
        }

        public async Task<PatientMedicalRecordResponseDTO> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecordRequestDTO record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            // Validation: PtnId must be positive
            if (record.PtnId <= 0)
                throw new ArgumentException("ID bệnh nhân (PtnId) phải là số dương.", nameof(record.PtnId));

            // Retrieve the existing entity
            var existingRecord = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(id);
            if (existingRecord == null)
                throw new KeyNotFoundException($"Hồ sơ bệnh án với id {id} không thể tìm thấy.");

            // Optionally, prevent changing to a PtnId that already has a record (if business logic requires)
            // var otherRecord = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(record.PtnId);
            // if (otherRecord != null && otherRecord.PmrId != id)
            //     throw new InvalidOperationException($"A medical record for patient ID {record.PtnId} already exists.");

            // Update properties
            existingRecord.PtnId = record.PtnId;

            // Update in repository
            var updatedEntity = await _patientMedicalRecordRepo.UpdatePatientMedicalRecordAsync(id, existingRecord);
            return MapToResponseDTO(updatedEntity);
        }

        public async Task<PatientMedicalRecordResponseDTO?> GetPersonalMedicalRecordAsync(int accId)
        {
            // Validation: patientId must be positive
            if (accId <= 0)
                throw new ArgumentException("ID tài khoản (accId) phải là số dương.", nameof(accId));

            // Retrieve the personal medical record from the repository
            var record = await _patientMedicalRecordRepo.GetPersonalMedicalRecordAsync(accId);
            if (record == null)
                return null;

            return MapToResponseDTO(record);
        }
    }
}
