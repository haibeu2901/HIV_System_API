using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IVerificationCodeService _verificationService;
        private readonly IMemoryCache _memoryCache;
        private const int PENDING_REGISTRATION_EXPIRY_MINUTES = 30;

        public AccountService(IMemoryCache memoryCache)
        {
            _accountRepo = new AccountRepo();
            _verificationService = new VerificationCodeService(memoryCache);
            _memoryCache = memoryCache;
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
                throw new InvalidOperationException($"Account already exists.");

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

        public async Task<AccountResponseDTO> UpdateAccountByIdAsync(int id, UpdateAccountRequestDTO updatedAccount)
        {
            if (updatedAccount == null)
                throw new ArgumentNullException(nameof(updatedAccount));
            //check authorization
            if (updatedAccount.Roles != 1)
            {
                if (updatedAccount.Roles == 4 && updatedAccount.Roles == 5)
                {
                    var currentAccount = await _accountRepo.GetAccountByIdAsync(id);
                    if (currentAccount?.Roles == 1)
                        throw new UnauthorizedAccessException("You do not have permission to update this account.");
                }
            }


            // Fetch the existing account
            var existingAccount = await _accountRepo.GetAccountByIdAsync(id);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Account with id {id} not found.");

            // Update fields
            existingAccount.AccPassword = updatedAccount.AccPassword;
            if (!string.IsNullOrWhiteSpace(updatedAccount.Email) && await _accountRepo.IsEmailUsedAsync(updatedAccount.Email))
            {
                throw new InvalidOperationException($"Email '{updatedAccount.Email}' is already in use.");
            }
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

        internal async Task<PatientResponseDTO> CreatePatientAccountAsync(PatientAccountRequestDTO patient)
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
                throw new InvalidOperationException($"Account already exists.");

            //Check if email is already used
            if (!string.IsNullOrWhiteSpace(patient.Email) && await _accountRepo.IsEmailUsedAsync(patient.Email))
            {
                throw new InvalidOperationException($"Email '{patient.Email}' is already in use.");
            }

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

            // Create Patient entity
            var patientEntity = new Patient
            {
                PtnId = createdAccount.AccId,
                AccId = createdAccount.AccId,
                Acc = createdAccount,
            };

            // Save Patient entity
            var patientRepo = new PatientRepo();
            var createdPatient = await patientRepo.CreatePatientAsync(patientEntity);

            // Map to PatientResponseDTO
            return new PatientResponseDTO
            {
                PatientId = createdPatient.PtnId,
                AccId = createdPatient.AccId,
                Account = MapToResponseDTO(createdAccount),
            };
        }

        public async Task<AccountResponseDTO> UpdatePatientProfileAsync(int accountId, PatientProfileUpdateDTO profileDTO)
        {
            if (profileDTO == null)
                throw new ArgumentNullException(nameof(profileDTO));

            var existingAccount = await _accountRepo.GetAccountByIdAsync(accountId);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            if (existingAccount.Roles != 3)
                throw new UnauthorizedAccessException("This account is not a patient account.");

            var accountToUpdate = new Account
            {
                AccId = accountId,
                AccUsername = existingAccount.AccUsername, // preserve username
                AccPassword = profileDTO.AccPassword ?? existingAccount.AccPassword,
                Email = profileDTO.Email ?? existingAccount.Email,
                Fullname = profileDTO.Fullname ?? existingAccount.Fullname,
                Dob = profileDTO.Dob ?? existingAccount.Dob,
                Gender = profileDTO.Gender ?? existingAccount.Gender,
                Roles = existingAccount.Roles, // preserve role
                IsActive = existingAccount.IsActive // preserve status
            };

            var updatedAccount = await _accountRepo.UpdateAccountProfileAsync(accountId, accountToUpdate);
            return MapToResponseDTO(updatedAccount);
        }

        public async Task<AccountResponseDTO> UpdateDoctorProfileAsync(int accountId, DoctorProfileUpdateDTO profileDTO)
        {
            if (profileDTO == null)
                throw new ArgumentNullException(nameof(profileDTO));

            var existingAccount = await _accountRepo.GetAccountByIdAsync(accountId);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            if (existingAccount.Roles != 2)
                throw new UnauthorizedAccessException("This account is not a doctor account.");

            var accountToUpdate = new Account
            {
                AccId = accountId,
                AccUsername = existingAccount.AccUsername, // preserve username
                AccPassword = profileDTO.AccPassword ?? existingAccount.AccPassword,
                Email = profileDTO.Email ?? existingAccount.Email,
                Fullname = profileDTO.Fullname ?? existingAccount.Fullname,
                Dob = profileDTO.Dob ?? existingAccount.Dob,
                Gender = profileDTO.Gender ?? existingAccount.Gender,
                Roles = existingAccount.Roles, // preserve role
                IsActive = existingAccount.IsActive // preserve status
            };

            // Update account profile
            var updatedAccount = await _accountRepo.UpdateAccountProfileAsync(accountId, accountToUpdate);

            // Update doctor-specific information
            if (!string.IsNullOrWhiteSpace(profileDTO.Degree) || !string.IsNullOrWhiteSpace(profileDTO.Bio))
            {
                var doctorRepo = new DoctorRepo();
                var doctor = await doctorRepo.GetDoctorByIdAsync(accountId);
                if (doctor != null)
                {
                    doctor.Degree = profileDTO.Degree ?? doctor.Degree;
                    doctor.Bio = profileDTO.Bio ?? doctor.Bio;
                    await doctorRepo.UpdateDoctorAsync(doctor.DctId, doctor);
                }
            }

            return MapToResponseDTO(updatedAccount);
        }

        

        public async Task<AccountResponseDTO?> GetAccountByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            var account = await _accountRepo.GetAccountByEmailAsync(email);
            if (account == null)
                return null;

            return MapToResponseDTO(account);
        }

        public async Task<(string verificationCode, string email)> InitiatePatientRegistrationAsync(PatientAccountRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.AccUsername))
                throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(request.AccPassword))
                throw new ArgumentException("Password is required");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            // Check for existing account
            var existingAccount = await _accountRepo.GetAccountByLoginAsync(request.AccUsername, request.AccPassword);
            if (existingAccount != null)
                throw new InvalidOperationException("Account already exists");

            if (await _accountRepo.IsEmailUsedAsync(request.Email))
                throw new InvalidOperationException($"Email '{request.Email}' is already in use");

            // Store the registration request in cache
            var pendingRegistration = new PendingPatientRegistrationDTO
            {
                AccUsername = request.AccUsername,
                AccPassword = request.AccPassword,
                Email = request.Email,
                Fullname = request.Fullname,
                Dob = request.Dob,
                Gender = request.Gender
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(PENDING_REGISTRATION_EXPIRY_MINUTES));

            var verificationCode = _verificationService.GenerateCode(request.Email);
            _memoryCache.Set($"pending_registration_{request.Email}", pendingRegistration, cacheOptions);

            return (verificationCode, request.Email);
        }

        public async Task<AccountResponseDTO> VerifyEmailAndCreateAccountAsync(string email, string code)
        {
            if (!_verificationService.VerifyCode(email, code))
                throw new InvalidOperationException("Invalid or expired verification code");

            // Retrieve pending registration
            var cacheKey = $"pending_registration_{email}";
            if (!_memoryCache.TryGetValue(cacheKey, out PendingPatientRegistrationDTO? pendingRegistration))
                throw new InvalidOperationException("No pending registration found or registration expired");

            _memoryCache.Remove(cacheKey);

            // Create the patient account
            var patientRequest = new PatientAccountRequestDTO
            {
                AccUsername = pendingRegistration.AccUsername,
                AccPassword = pendingRegistration.AccPassword,
                Email = pendingRegistration.Email,
                Fullname = pendingRegistration.Fullname,
                Dob = pendingRegistration.Dob,
                Gender = pendingRegistration.Gender
            };

            var patientResponse = await CreatePatientAccountAsync(patientRequest);

            // The account is created with IsActive = true since it's already verified
            var account = await _accountRepo.GetAccountByIdAsync(patientResponse.AccId);
            if (account != null)
            {
                account.IsActive = true;
                await _accountRepo.UpdateAccountStatusAsync(account.AccId, true);
            }

            return patientResponse.Account ?? throw new InvalidOperationException("Failed to create account");
        }

        public async Task<(bool isValid, string message)> HasPendingRegistrationAsync(string email)
        {
            // Use the same key pattern as used in InitiatePatientRegistrationAsync
            var pendingRegistration = _memoryCache.Get<PendingPatientRegistrationDTO>($"pending_registration_{email}");

            if (pendingRegistration == null)
                return (false, "No pending registration found for this email");

            // Check if there's already an account with this email
            var existingAccount = await GetAccountByEmailAsync(email);
            if (existingAccount != null)
            {
                if (existingAccount.IsActive == true)
                    return (false, "Account is already verified");
                return (true, ""); // Account exists but not verified
            }

            return (true, ""); // Pending registration exists
        }
    }
}
