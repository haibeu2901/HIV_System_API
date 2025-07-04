﻿using HIV_System_API_BOs;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IVerificationCodeService _verificationService;
        private readonly IMemoryCache _memoryCache;
        private const int PENDING_REGISTRATION_EXPIRY_MINUTES = 30;
        // Define a reasonable, consistent timeout to prevent Regular Expression Denial of Service (ReDoS) attacks.
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

        public AccountService(IMemoryCache memoryCache)
        {
            _accountRepo = new AccountRepo();
            _verificationService = new VerificationCodeService(memoryCache);
            _memoryCache = memoryCache;
        }

        public AccountService(IAccountRepo accountRepo, IVerificationCodeService verificationService, IMemoryCache memoryCache)
        {
            _accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
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

        /// <summary>
        /// Validates a username based on a set of rules.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="email">Optional: The user's email to ensure the username is not the same.</param>
        /// <exception cref="ArgumentException">Thrown when the username is invalid.</exception>
        public static void ValidateUsername(string username, string email = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));

            if (username.Length < 3 || username.Length > 16)
                throw new ArgumentException("Username must be between 3 and 16 characters.", nameof(username));

            try
            {
                // This regex ensures the username only contains allowed characters.
                // A timeout is specified to prevent ReDoS attacks.
                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$", RegexOptions.None, RegexTimeout))
                    throw new ArgumentException("Username can only contain letters, numbers, underscores, and hyphens.", nameof(username));
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle the case where the regex match takes too long.
                throw new ArgumentException("Username validation timed out due to excessive complexity.", nameof(username));
            }

            if (!string.IsNullOrEmpty(email) && username.Equals(email, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Username cannot be the same as the email.", nameof(username));
        }

        /// <summary>
        /// Validates a password based on complexity and length rules.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <param name="username">Optional: The user's username to ensure the password is not the same.</param>
        /// <exception cref="ArgumentException">Thrown when the password is invalid.</exception>
        public static void ValidatePassword(string password, string username = null)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));

            if (password.Contains(' '))
                throw new ArgumentException("Password cannot contain spaces.", nameof(password));

            // Use LINQ for better readability than a complex regex with lookaheads.
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            const string specialCharacters = @"~!@#$%^&*.";
            bool hasSpecialChar = password.Any(specialCharacters.Contains);

            if (!hasUppercase || !hasLowercase || !hasDigit || !hasSpecialChar)
                throw new ArgumentException($"Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character ({specialCharacters}).", nameof(password));

            if (!string.IsNullOrEmpty(username) && password.Equals(username, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Password cannot be the same as the username.", nameof(password));
        }

        /// <summary>
        /// Validates an email address based on format and length rules.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <param name="username">Optional: The user's username to ensure the email is not the same.</param>
        /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
        public static void ValidateEmail(string email, string username = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (email.Length > 254)
                throw new ArgumentException("Email must not exceed 254 characters.", nameof(email));

            var parts = email.Split('@');
            if (parts.Length != 2)
                throw new ArgumentException("Email format is invalid (must contain exactly one '@').", nameof(email));

            var localPart = parts[0];
            var domainPart = parts[1];

            if (localPart.Length == 0 || localPart.Length > 64)
                throw new ArgumentException("Email local-part must be between 1 and 64 characters.", nameof(email));

            if (domainPart.Length == 0 || domainPart.Length > 255)
                throw new ArgumentException("Email domain must be between 1 and 255 characters.", nameof(email));

            if (localPart.Contains("..") || domainPart.Contains(".."))
                throw new ArgumentException("Email cannot contain consecutive dots.", nameof(email));

            try
            {
                // Use a practical regex for overall format validation with a timeout to prevent ReDoS.
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.None, RegexTimeout))
                    throw new ArgumentException("Email format is invalid.", nameof(email));
            }
            catch (RegexMatchTimeoutException)
            {
                throw new ArgumentException("Email validation timed out due to excessive complexity.", nameof(email));
            }

            if (!string.IsNullOrEmpty(username) && email.Equals(username, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email cannot be the same as the username.", nameof(email));
        }

        public async Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            // Validate username, password, and email
            ValidateUsername(account.AccUsername, account.Email);
            ValidatePassword(account.AccPassword, account.AccUsername);
            ValidateEmail(account.Email, account.AccUsername);

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
            if (string.IsNullOrWhiteSpace(accUsername) || string.IsNullOrWhiteSpace(accPassword))
                return null;

            // Validate username and password format
            ValidateUsername(accUsername);
            ValidatePassword(accPassword, accUsername);

            var account = await _accountRepo.GetAccountByLoginAsync(accUsername, accPassword);
            return account == null ? null : MapToResponseDTO(account);
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

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(updatedAccount.AccPassword))
                ValidatePassword(updatedAccount.AccPassword);

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(updatedAccount.Email))
                ValidateEmail(updatedAccount.Email);

            // Check authorization
            if (updatedAccount.Roles != 1)
            {
                if (updatedAccount.Roles == 4 || updatedAccount.Roles == 5)
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

            // Check email uniqueness if updated
            if (!string.IsNullOrWhiteSpace(updatedAccount.Email) && !updatedAccount.Email.Equals(existingAccount.Email, StringComparison.OrdinalIgnoreCase) && await _accountRepo.IsEmailUsedAsync(updatedAccount.Email))
            {
                throw new InvalidOperationException($"Email '{updatedAccount.Email}' is already in use.");
            }

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

        internal async Task<PatientResponseDTO> CreatePatientAccountAsync(PatientAccountRequestDTO patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            if (string.IsNullOrWhiteSpace(patient.AccUsername))
                throw new ArgumentNullException(nameof(patient.AccUsername));
            if (string.IsNullOrWhiteSpace(patient.AccPassword))
                throw new ArgumentNullException(nameof(patient.AccPassword));

            // Validate username, password, and email
            ValidateUsername(patient.AccUsername, patient.Email);
            ValidatePassword(patient.AccPassword, patient.AccUsername);
            ValidateEmail(patient.Email, patient.AccUsername);

            // Check for duplicate username
            if (!string.IsNullOrWhiteSpace(patient.AccUsername))
            {
                var existingAccount = await _accountRepo.GetAccountByUsernameAsync(patient.AccUsername);
                if (existingAccount != null)
                {
                    throw new InvalidOperationException($"Username '{patient.AccUsername}' is already in use.");
                }
            }

            // Check if email is already used
            if (!string.IsNullOrWhiteSpace(patient.Email) && await _accountRepo.IsEmailUsedAsync(patient.Email))
            {
                throw new InvalidOperationException($"Email '{patient.Email}' is already in use.");
            }

            // Check date of birth
            if (patient.Dob.HasValue && patient.Dob.Value > DateTime.Now)
            {
                throw new ArgumentException("Date of birth cannot be in the future.", nameof(patient.Dob));
            }
            if (patient.Dob.HasValue && patient.Dob.Value < new DateTime(1900, 1, 1))
            {
                throw new ArgumentException("Date of birth cannot be before 1900.", nameof(patient.Dob));
            }

            // Check if patient is at least 18 years old
            if (patient.Dob.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - patient.Dob.Value.Year;
                if (patient.Dob.Value.Date > today.AddYears(-age)) age--;
                if (age < 18)
                {
                    throw new ArgumentException("Patient must be at least 18 years old.", nameof(patient.Dob));
                }
            }

            // Map PatientAccountRequestDTO to AccountRequestDTO
            var accountDto = new AccountRequestDTO
            {
                AccUsername = patient.AccUsername,
                AccPassword = patient.AccPassword,
                Email = patient.Email,
                Fullname = patient.Fullname,
                Dob = patient.Dob.HasValue ? DateOnly.FromDateTime(patient.Dob.Value) : null,
                Gender = patient.Gender,
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

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(profileDTO.Email))
                ValidateEmail(profileDTO.Email, existingAccount.AccUsername);

            // Check email uniqueness if updated
            if (!string.IsNullOrWhiteSpace(profileDTO.Email) && !profileDTO.Email.Equals(existingAccount.Email, StringComparison.OrdinalIgnoreCase) && await _accountRepo.IsEmailUsedAsync(profileDTO.Email))
            {
                throw new InvalidOperationException($"Email '{profileDTO.Email}' is already in use.");
            }

            var accountToUpdate = new Account
            {
                AccId = accountId,
                AccUsername = existingAccount.AccUsername, // preserve username
                AccPassword = existingAccount.AccPassword,
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

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(profileDTO.AccPassword))
                ValidatePassword(profileDTO.AccPassword, existingAccount?.AccUsername);

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(profileDTO.Email))
                ValidateEmail(profileDTO.Email, existingAccount?.AccUsername);

            // Check email uniqueness if updated
            if (!string.IsNullOrWhiteSpace(profileDTO.Email) && !profileDTO.Email.Equals(existingAccount.Email, StringComparison.OrdinalIgnoreCase) && await _accountRepo.IsEmailUsedAsync(profileDTO.Email))
            {
                throw new InvalidOperationException($"Email '{profileDTO.Email}' is already in use.");
            }

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

            // Validate email
            ValidateEmail(email);

            var account = await _accountRepo.GetAccountByEmailAsync(email);
            if (account == null)
                return null;

            return MapToResponseDTO(account);
        }

        public async Task<string> InitiatePasswordResetAsync(ForgotPasswordRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            // Validate email
            ValidateEmail(request.Email);

            // Check if account exists for the email
            var account = await _accountRepo.GetAccountByEmailAsync(request.Email);
            if (account == null)
                throw new InvalidOperationException("No account found with the provided email.");

            // Generate verification code
            var verificationCode = _verificationService.GenerateCode(request.Email);
            // Store the code in cache with expiry
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(PENDING_REGISTRATION_EXPIRY_MINUTES));
            _memoryCache.Set($"forgot_password_{request.Email}", verificationCode, cacheOptions);
            // TODO: Send code to user's email (email sending not implemented here)
            return verificationCode;
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

            // Validate username, password, and email
            ValidateUsername(request.AccUsername, request.Email);
            ValidatePassword(request.AccPassword, request.AccUsername);
            ValidateEmail(request.Email, request.AccUsername);

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

            // Validate email
            ValidateEmail(email);

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
            // Validate email
            ValidateEmail(email);

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

        public async Task<bool> ChangePasswordAsync(int accId, ChangePasswordRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.currentPassword))
                throw new ArgumentException("Current password is required.", nameof(request.currentPassword));
            if (string.IsNullOrWhiteSpace(request.newPassword))
                throw new ArgumentException("New password is required.", nameof(request.newPassword));
            if (string.IsNullOrWhiteSpace(request.confirmNewPassword))
                throw new ArgumentException("Confirm new password is required.", nameof(request.confirmNewPassword));

            if (request.newPassword != request.confirmNewPassword)
                throw new InvalidOperationException("New password and confirmation do not match.");

            var account = await _accountRepo.GetAccountByIdAsync(accId);
            if (account == null)
                throw new KeyNotFoundException($"Account with id {accId} not found.");

            if (account.AccPassword != request.currentPassword)
                throw new InvalidOperationException("Current password is incorrect.");

            // Validate new password
            ValidatePassword(request.newPassword, account.AccUsername);

            if (account.AccPassword == request.newPassword)
                throw new InvalidOperationException("New password must be different from the current password.");

            // Update password
            var changeRequest = new ChangePasswordRequestDTO
            {
                currentPassword = request.currentPassword,
                newPassword = request.newPassword,
                confirmNewPassword = request.confirmNewPassword
            };

            var result = await _accountRepo.ChangePasswordAsync(accId, changeRequest);
            return result;
        }

        public async Task<(bool Success, string Message)> SendPasswordResetCodeAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");

            // Validate email
            ValidateEmail(email);

            // Check if account exists for the email
            var account = await _accountRepo.GetAccountByEmailAsync(email);
            if (account == null)
                return (false, "No account found with the provided email.");

            // Optionally: Check if account is active
            if (account.IsActive == false)
                return (false, "Account is not active.");

            // Generate verification code
            var code = _verificationService.GenerateCode(email);

            // Store the code in cache with expiry
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(PENDING_REGISTRATION_EXPIRY_MINUTES));
            _memoryCache.Set($"password_reset_{email}", code, cacheOptions);

            // TODO: Send code to user's email (email sending not implemented here)
            return (true, "Password reset code has been sent to your email.");
        }

        public async Task<(bool Success, string Message)> VerifyPasswordResetCodeAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Verification code is required.");

            // Validate email
            ValidateEmail(email);

            // Check if the code exists in cache (for password reset)
            var cacheKey = $"forgot_password_{email}";
            if (!_memoryCache.TryGetValue<string>(cacheKey, out var cachedCode))
                return (false, "No password reset request found or code expired.");

            if (!string.Equals(cachedCode, code, StringComparison.Ordinal))
                return (false, "Verification code does not match.");

            // Optionally: Remove the code from cache after successful verification
            _memoryCache.Remove(cacheKey);

            return (true, "Verification code is valid.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Verification code is required.");
            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "New password is required.");

            // Validate email
            ValidateEmail(email);

            var account = await _accountRepo.GetAccountByEmailAsync(email);
            if (account == null)
                return (false, "No account found with the provided email.");
            if (account.IsActive == false)
                return (false, "Account is not active.");

            var (isValid, message) = await VerifyPasswordResetCodeAsync(email, code);
            if (!isValid)
                return (false, message);

            // Validate new password
            ValidatePassword(newPassword, account.AccUsername);

            if (account.AccPassword == newPassword)
                return (false, "New password must be different from the current password.");

            var changeRequest = new ChangePasswordRequestDTO
            {
                currentPassword = account.AccPassword,
                newPassword = newPassword,
                confirmNewPassword = newPassword
            };

            var result = await _accountRepo.ChangePasswordAsync(account.AccId, changeRequest);
            if (!result)
                return (false, "Failed to reset password.");

            return (true, "Password has been reset successfully.");
        }
    }
}