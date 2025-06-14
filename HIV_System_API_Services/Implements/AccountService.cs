using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
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
    public class AccountService : IAccountService
    {
        private readonly IAccountRepo _accountRepo;

        public AccountService()
        {
            _accountRepo = new AccountRepo();
        }

        private Account MapToEntity(AccountRequestDTO dto)
        {
            return new Account
            {
                AccUsername = dto.AccUsername,
                AccPassword = dto.AccPassword,
                Email = dto.Email,
                Fullname = dto.Fullname,
                Dob = dto.Dob,
                Gender = dto.Gender,
                Roles = dto.Roles,
                IsActive = dto.IsActive
            };
        }

        private AccountResponseDTO MapToResponseDTO(Account account)
        {
            return new AccountResponseDTO
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
            };
        }

        public async Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            // Check for duplicate username
            var existing = await _accountRepo.GetAccountByLoginAsync(account.AccUsername, account.AccPassword);
            if (existing != null)
                throw new InvalidOperationException($"Account with username '{account.AccUsername}' already exists.");

            var entity = MapToEntity(account);
            var createdAccount = await _accountRepo.CreateAccountAsync(entity);
            return MapToResponseDTO(createdAccount);
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            return await _accountRepo.DeleteAccountAsync(accId);
        }

        public async Task<AccountResponseDTO?> GetAccountByIdAsync(int accId)
        {
            var account = await _accountRepo.GetAccountByIdAsync(accId);
            if (account == null)
                return null;
            return MapToResponseDTO(account);
        }

        public async Task<AccountResponseDTO?> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            var account = await _accountRepo.GetAccountByLoginAsync(accUsername, accPassword);
            if (account == null)
                return null;
            return MapToResponseDTO(account);
        }

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync()
        {
            var accounts = await _accountRepo.GetAllAccountsAsync();
            return accounts.Select(MapToResponseDTO).ToList();
        }

        public async Task<AccountResponseDTO> UpdateAccountByIdAsync(int id, AccountRequestDTO updatedAccount)
        {
            if (updatedAccount == null)
                throw new ArgumentNullException(nameof(updatedAccount));

            // Fetch the existing account
            var existingAccount = await _accountRepo.GetAccountByIdAsync(id);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Account with id {id} not found.");

            // Update fields
            existingAccount.AccPassword = updatedAccount.AccPassword;
            existingAccount.Email = updatedAccount.Email;
            existingAccount.Fullname = updatedAccount.Fullname;
            existingAccount.Dob = updatedAccount.Dob;
            existingAccount.Gender = updatedAccount.Gender;
            existingAccount.Roles = updatedAccount.Roles;
            existingAccount.IsActive = updatedAccount.IsActive;

            // Save changes
            var updatedEntity = await _accountRepo.UpdateAccountByIdAsync(id, existingAccount);
            return MapToResponseDTO(updatedEntity);
        }

        public async Task<PatientResponseDTO> CreatePatientAccountAsync(PatientAccountRequestDTO patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            if (string.IsNullOrWhiteSpace(patient.AccUsername))
                throw new ArgumentNullException(nameof(patient.AccUsername));
            if (string.IsNullOrWhiteSpace(patient.AccPassword))
                throw new ArgumentNullException(nameof(patient.AccPassword));

            // Check for duplicate username (by username only, not password)
            var existingAccount = await _accountRepo.GetAccountByLoginAsync(patient.AccUsername, patient.AccPassword);
            if (existingAccount != null)
                throw new InvalidOperationException($"Account with username '{patient.AccUsername}' already exists.");

            // Map PatientAccountRequestDTO to AccountRequestDTO
            var accountDto = new AccountRequestDTO
            {
                AccUsername = patient.AccUsername,
                AccPassword = patient.AccPassword,
                Email = patient.Email,
                Fullname = patient.Fullname,
                Dob = patient.Dob.HasValue ? DateOnly.FromDateTime(patient.Dob.Value) : null,
                Gender = patient.Gender, // PatientAccountRequestDTO does not have Gender, set to null or default
                Roles = 3, // Assuming 3 is the role for Patient
                IsActive = true
            };

            // Create Account
            var createdAccount = await _accountRepo.CreateAccountAsync(MapToEntity(accountDto));

            // Create PatientMedicalRecord
            var patientMedicalRecord = new PatientMedicalRecord
            {
                PtnId = createdAccount.AccId
            };

            // Create Patient entity
            var patientEntity = new Patient
            {
                PtnId = createdAccount.AccId,
                AccId = createdAccount.AccId,
                Acc = createdAccount,
                PatientMedicalRecord = patientMedicalRecord
            };

            // Save Patient entity
            var patientRepo = new PatientRepo();
            var createdPatient = await patientRepo.CreatePatientAsync(patientEntity);

            // Map to PatientMedicalRecordResponseDTO
            var medicalRecordDto = new PatientMedicalRecordResponseDTO
            {
                PmrId = createdPatient.PatientMedicalRecord?.PmrId ?? 0,
                PtnId = createdPatient.PtnId
            };

            // Map to PatientResponseDTO
            return new PatientResponseDTO
            {
                PtnId = createdPatient.PtnId,
                AccId = createdPatient.AccId,
                Account = MapToResponseDTO(createdAccount),
                MedicalRecord = medicalRecordDto
            };
        }
    }
}
