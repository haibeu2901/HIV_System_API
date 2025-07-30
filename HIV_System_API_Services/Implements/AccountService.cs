using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.DoctorDTO;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using HIV_System_API_DTOs.StaffDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private const int SaltLength = 12; // 12 bytes -> 16 chars in base64
        private const int HashLength = 24; // 24 bytes -> 32 chars in base64
        private const int Iterations = 100000; // PBKDF2 iteration count

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

        private string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
            // Generate PBKDF2 hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashLength);
                // Combine salt and hash as base64 strings
                string saltBase64 = Convert.ToBase64String(salt);
                string hashBase64 = Convert.ToBase64String(hash);
                return $"{saltBase64}:{hashBase64}"; // Format: <16-chars>:<32-chars>
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // Split stored hash into salt and hash
            string[] parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

                // Compute hash of provided password
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashLength);
                    // Compare hashes
                    return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private Account MapToEntity(AccountRequestDTO dto)
        {
            return new Account
            {
                AccUsername = dto.AccUsername,
                AccPassword = HashPassword(dto.AccPassword),
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
                // Exclude AccPassword for security
                Email = account.Email,
                Fullname = account.Fullname,
                Dob = account.Dob,
                Gender = account.Gender,
                Roles = account.Roles,
                IsActive = account.IsActive
            };
        }

        /// <summary>
        /// Validates a username based on a set of rules and checks for duplicates.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="email">Optional: The user's email to ensure the username is not the same.</param>
        /// <param name="excludeAccountId">Optional: Account ID to exclude from duplicate check (for updates).</param>
        /// <exception cref="ArgumentException">Thrown when the username is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the username is already in use.</exception>
        public async Task ValidateUsernameAsync(string username, string email = null, int? excludeAccountId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Tên người dùng là bắt buộc.", nameof(username));

            if (username.Length < 3 || username.Length > 24)
                throw new ArgumentException("Tên người dùng phải có từ 3 đến 24 ký tự.", nameof(username));

            try
            {
                // This regex ensures the username only contains allowed characters.
                // A timeout is specified to prevent ReDoS attacks.
                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$", RegexOptions.None, RegexTimeout))
                    throw new ArgumentException("Tên người dùng chỉ được chứa chữ cái, số, dấu gạch dưới và dấu gạch ngang.", nameof(username));
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle the case where the regex match takes too long.
                throw new ArgumentException("Việc xác thực tên người dùng đã hết thời gian do độ phức tạp quá cao.", nameof(username));
            }

            if (!string.IsNullOrEmpty(email) && username.Equals(email, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Tên người dùng không được trùng với email.", nameof(username));

            // Check for duplicate username using repository
            var existingAccount = await _accountRepo.GetAccountByUsernameAsync(username);
            if (existingAccount != null)
            {
                // If excludeAccountId is provided, check if it's the same account (for updates)
                if (excludeAccountId.HasValue && existingAccount.AccId == excludeAccountId.Value)
                {
                    // This is the same account being updated, so username is allowed
                    return;
                }

                throw new InvalidOperationException($"Tên người dùng '{username}' đã được sử dụng.");
            }
        }

        /// <summary>
        /// Static version of ValidateUsername for backward compatibility (without duplicate checking).
        /// Use ValidateUsernameAsync for complete validation including duplicate checking.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="email">Optional: The user's email to ensure the username is not the same.</param>
        /// <exception cref="ArgumentException">Thrown when the username is invalid.</exception>
        public static void ValidateUsername(string username, string email = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Tên người dùng là bắt buộc.", nameof(username));

            if (username.Length < 3 || username.Length > 24)
                throw new ArgumentException("Tên người dùng phải có từ 3 đến 24 ký tự.", nameof(username));

            try
            {
                // This regex ensures the username only contains allowed characters.
                // A timeout is specified to prevent ReDoS attacks.
                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$", RegexOptions.None, RegexTimeout))
                    throw new ArgumentException("Tên người dùng chỉ được chứa chữ cái, số, dấu gạch dưới và dấu gạch ngang.", nameof(username));
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle the case where the regex match takes too long.
                throw new ArgumentException("Việc xác thực tên người dùng đã hết thời gian do độ phức tạp quá cao.", nameof(username));
            }

            if (!string.IsNullOrEmpty(email) && username.Equals(email, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Tên người dùng không được trùng với email.", nameof(username));
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
                throw new ArgumentException("Mật khẩu là bắt buộc.", nameof(password));

            if (password.Length < 8)
                throw new ArgumentException("Mật khẩu phải có ít nhất 8 ký tự.", nameof(password));

            if (password.Length > 50)
                throw new ArgumentException("Mật khẩu không được quá 50 ký tự.", nameof(password));

            if (password.Contains(' '))
                throw new ArgumentException("Mật khẩu không được chứa dấu cách.", nameof(password));

            // Use LINQ for better readability than a complex regex with lookaheads.
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            const string specialCharacters = @"~`!@#$%^&*.,/()-_+=[]{}\|;:<>?";
            bool hasSpecialChar = password.Any(specialCharacters.Contains);

            if (!hasUppercase || !hasLowercase || !hasDigit || !hasSpecialChar)
                throw new ArgumentException($"Mật khẩu phải chứa ít nhất một chữ hoa, một chữ thường, một số và một ký tự đặc biệt ({specialCharacters}).", nameof(password));

            if (!string.IsNullOrEmpty(username) && password.Equals(username, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Mật khẩu không được trùng với tên người dùng.", nameof(password));
        }

        /// <summary>
        /// Validates an email address based on format and length rules and checks for duplicates.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <param name="username">Optional: The user's username to ensure the email is not the same.</param>
        /// <param name="excludeAccountId">Optional: Account ID to exclude from duplicate check (for updates).</param>
        /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the email is already in use.</exception>
        public async Task ValidateEmailAsync(string email, string username = null, int? excludeAccountId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email là bắt buộc.", nameof(email));

            if (email.Length > 100)
                throw new ArgumentException("Email không được vượt quá 100 ký tự.", nameof(email));

            var parts = email.Split('@');
            if (parts.Length != 2)
                throw new ArgumentException("Định dạng email không hợp lệ (phải chứa chính xác một ký tự '@').", nameof(email));

            var localPart = parts[0];
            var domainPart = parts[1];

            if (localPart.Length == 0 || localPart.Length > 32)
                throw new ArgumentException("Phần cục bộ của email phải có từ 1 đến 32 ký tự.", nameof(email));

            if (domainPart.Length == 0 || domainPart.Length > 63)
                throw new ArgumentException("Tên miền của email phải có từ 1 đến 63 ký tự.", nameof(email));

            if (localPart.Contains("..") || domainPart.Contains(".."))
                throw new ArgumentException("Email không được chứa các dấu chấm liên tiếp.", nameof(email));

            try
            {
                // Use a practical regex for overall format validation with a timeout to prevent ReDoS.
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.None, RegexTimeout))
                    throw new ArgumentException("Định dạng email không hợp lệ.", nameof(email));
            }
            catch (RegexMatchTimeoutException)
            {
                throw new ArgumentException("Việc xác thực email đã hết thời gian do độ phức tạp quá cao.", nameof(email));
            }

            if (!string.IsNullOrEmpty(username) && email.Equals(username, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email không được trùng với tên người dùng.", nameof(username));

            // Check for duplicate email using repository
            var existingAccount = await _accountRepo.GetAccountByEmailAsync(email);
            if (existingAccount != null)
            {
                // If excludeAccountId is provided, check if it's the same account (for updates)
                if (excludeAccountId.HasValue && existingAccount.AccId == excludeAccountId.Value)
                {
                    // This is the same account being updated, so email is allowed
                    return;
                }

                throw new InvalidOperationException($"Email '{email}' đã được sử dụng.");
            }
        }

        private static void ValidateDateOfBirth(DateTime? dob, string paramName = "dob")
        {
            if (!dob.HasValue)
                throw new ArgumentException("Ngày sinh là bắt buộc.", paramName);

            var dobValue = dob.Value;
            var today = DateTime.Today;

            // Check if DOB is not in the future
            if (dobValue.Date > today)
                throw new ArgumentException("Ngày sinh không được ở trong tương lai.", paramName);

            // Check if DOB is not before 1900
            if (dobValue < new DateTime(1900, 1, 1))
                throw new ArgumentException("Ngày sinh không được trước năm 1900.", paramName);

            // Check if person is at least 18 years old - FIXED LOGIC
            var age = CalculateAge(dobValue.Date, today);
            if (age < 18)
                throw new ArgumentException("Người dùng phải ít nhất 18 tuổi.", paramName);
        }

        /// <summary>
        /// Calculates the exact age based on date of birth and reference date
        /// </summary>
        /// <param name="dateOfBirth">The date of birth</param>
        /// <param name="referenceDate">The reference date (usually today)</param>
        /// <returns>The age in years</returns>
        private static int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
        {
            int age = referenceDate.Year - dateOfBirth.Year;

            // Check if the birthday hasn't occurred yet this year
            if (referenceDate < dateOfBirth.AddYears(age))
                age--;

            return age;
        }

        // Updated overload for DateTime parameter
        private static void ValidateDateOfBirth(DateTime dob, string paramName = "dob")
        {
            ValidateDateOfBirth((DateTime?)dob, paramName);
        }

        /// <summary>
        /// Static version of ValidateEmail for backward compatibility (without duplicate checking).
        /// Use ValidateEmailAsync for complete validation including duplicate checking.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <param name="username">Optional: The user's username to ensure the email is not the same.</param>
        /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
        public static void ValidateEmail(string email, string username = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email là bắt buộc.", nameof(email));

            if (email.Length > 254)
                throw new ArgumentException("Email không được vượt quá 254 ký tự.", nameof(email));

            var parts = email.Split('@');
            if (parts.Length != 2)
                throw new ArgumentException("Định dạng email không hợp lệ (phải chứa chính xác một ký tự '@').", nameof(email));

            var localPart = parts[0];
            var domainPart = parts[1];

            if (localPart.Length == 0 || localPart.Length > 64)
                throw new ArgumentException("Phần cục bộ của email phải có từ 1 đến 64 ký tự.", nameof(email));

            if (domainPart.Length == 0 || domainPart.Length > 255)
                throw new ArgumentException("Tên miền của email phải có từ 1 đến 255 ký tự.", nameof(email));

            if (localPart.Contains("..") || domainPart.Contains(".."))
                throw new ArgumentException("Email không được chứa các dấu chấm liên tiếp.", nameof(email));

            try
            {
                // Use a practical regex for overall format validation with a timeout to prevent ReDoS.
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.None, RegexTimeout))
                    throw new ArgumentException("Định dạng email không hợp lệ.", nameof(email));
            }
            catch (RegexMatchTimeoutException)
            {
                throw new ArgumentException("Việc xác thực email đã hết thời gian do độ phức tạp quá cao.", nameof(email));
            }

            if (!string.IsNullOrEmpty(username) && email.Equals(username, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email không được trùng với tên người dùng.", nameof(username));
        }

        public async Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            // Use new async validation methods with duplicate checking
            await ValidateUsernameAsync(account.AccUsername, account.Email);
            ValidatePassword(account.AccPassword, account.AccUsername);
            await ValidateEmailAsync(account.Email, account.AccUsername);

            // Validate Date of Birth - FIXED
            if (account.Dob.HasValue)
            {
                // Convert DateOnly to DateTime for validation
                var dobDateTime = account.Dob.Value.ToDateTime(TimeOnly.MinValue);
                ValidateDateOfBirth(dobDateTime, nameof(account.Dob));
            }

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

            // Use static validation for login (no need to check duplicates)
            ValidateUsername(accUsername);
            ValidatePassword(accPassword, accUsername);

            var account = await _accountRepo.GetAccountByUsernameAsync(accUsername);
            if (account == null || !VerifyPassword(accPassword, account.AccPassword))
                return null;

            return MapToResponseDTO(account);
        }

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync()
        {
            var accounts = await _accountRepo.GetAllAccountsAsync();
            return accounts.Select(MapToResponseDTO).ToList();
        }

        // Updated UpdateAccountByIdAsync method with DOB validation
        public async Task<AccountResponseDTO> UpdateAccountByIdAsync(int id, UpdateAccountRequestDTO updatedAccount)
        {
            if (updatedAccount == null)
                throw new ArgumentNullException(nameof(updatedAccount));

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(updatedAccount.AccPassword))
                ValidatePassword(updatedAccount.AccPassword);

            // Validate email if provided with duplicate checking, excluding current account
            if (!string.IsNullOrWhiteSpace(updatedAccount.Email))
                await ValidateEmailAsync(updatedAccount.Email, excludeAccountId: id);

            // Validate Date of Birth if provided - FIXED
            if (updatedAccount.Dob.HasValue)
            {
                var dobDateTime = updatedAccount.Dob.Value.ToDateTime(TimeOnly.MinValue);
                ValidateDateOfBirth(dobDateTime, nameof(updatedAccount.Dob));
            }

            // Check authorization
            if (updatedAccount.Roles != 1)
            {
                if (updatedAccount.Roles == 4 || updatedAccount.Roles == 5)
                {
                    var currentAccount = await _accountRepo.GetAccountByIdAsync(id);
                    if (currentAccount?.Roles == 1)
                        throw new UnauthorizedAccessException("Bạn không có quyền cập nhật tài khoản này.");
                }
            }

            // Fetch the existing account
            var existingAccount = await _accountRepo.GetAccountByIdAsync(id);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Không tìm thấy tài khoản với id {id}.");

            // Update fields
            existingAccount.AccPassword = !string.IsNullOrWhiteSpace(updatedAccount.AccPassword) ? HashPassword(updatedAccount.AccPassword) : existingAccount.AccPassword;
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

            // Validate required fields
            if (string.IsNullOrWhiteSpace(patient.AccUsername))
                throw new ArgumentException("Tên người dùng là bắt buộc.", nameof(patient.AccUsername));
            if (string.IsNullOrWhiteSpace(patient.AccPassword))
                throw new ArgumentException("Mật khẩu là bắt buộc.", nameof(patient.AccPassword));
            if (string.IsNullOrWhiteSpace(patient.Email))
                throw new ArgumentException("Email là bắt buộc.", nameof(patient.Email));
            if (!patient.Dob.HasValue)
                throw new ArgumentException("Ngày sinh là bắt buộc để đăng ký bệnh nhân.", nameof(patient.Dob));

            // Use async validation methods with duplicate checking
            await ValidateUsernameAsync(patient.AccUsername, patient.Email);
            ValidatePassword(patient.AccPassword, patient.AccUsername);
            await ValidateEmailAsync(patient.Email, patient.AccUsername);

            // Validate Date of Birth - Fixed to use consistent DateTime handling
            ValidateDateOfBirth(patient.Dob.Value, nameof(patient.Dob));

            // Map PatientAccountRequestDTO to AccountRequestDTO
            var accountDto = new AccountRequestDTO
            {
                AccUsername = patient.AccUsername,
                AccPassword = patient.AccPassword,
                Email = patient.Email,
                Fullname = patient.Fullname,
                Dob = patient.Dob.HasValue ? DateOnly.FromDateTime(patient.Dob.Value) : null,
                Gender = patient.Gender,
                Roles = 3, // 3 is the role for Patient
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

        /// <summary>
        /// Updates basic profile for Admin (role=1), Patient (role=3), Manager (role=5)
        /// Only allows updating: Fullname, Dob, Gender
        /// </summary>
        public async Task<BasicProfileResponseDTO> UpdateBasicProfileAsync(int accountId, BasicProfileUpdateDTO profileDTO)
        {
            if (profileDTO == null)
                throw new ArgumentNullException(nameof(profileDTO));

            var existingAccount = await _accountRepo.GetAccountByIdAsync(accountId);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Không tìm thấy tài khoản với id {accountId}.");

            // Validate that this is a basic profile role (1, 3, or 5)
            if (existingAccount.Roles != 1 && existingAccount.Roles != 3 && existingAccount.Roles != 5)
                throw new UnauthorizedAccessException("Vai trò tài khoản này không hỗ trợ cập nhật hồ sơ cơ bản.");

            // Validate Date of Birth if provided - FIXED
            if (profileDTO.Dob.HasValue)
            {
                var dobDateTime = profileDTO.Dob.Value.ToDateTime(TimeOnly.MinValue);
                ValidateDateOfBirth(dobDateTime, nameof(profileDTO.Dob));
            }

            // Create account object with only updated fields (basic profile fields only)
            var accountToUpdate = new Account
            {
                AccId = accountId,
                AccUsername = existingAccount.AccUsername,
                AccPassword = existingAccount.AccPassword,
                Email = existingAccount.Email,
                Fullname = profileDTO.Fullname ?? existingAccount.Fullname,
                Dob = profileDTO.Dob ?? existingAccount.Dob,
                Gender = profileDTO.Gender ?? existingAccount.Gender,
                Roles = existingAccount.Roles,
                IsActive = existingAccount.IsActive
            };

            var updatedAccount = await _accountRepo.UpdateAccountProfileAsync(accountId, accountToUpdate);

            return new BasicProfileResponseDTO
            {
                AccId = updatedAccount.AccId,
                AccUsername = updatedAccount.AccUsername,
                Email = updatedAccount.Email,
                Fullname = updatedAccount.Fullname,
                Dob = updatedAccount.Dob,
                Gender = updatedAccount.Gender,
                Roles = updatedAccount.Roles,
                IsActive = updatedAccount.IsActive
            };
        }

        public async Task<PatientProfileResponseDTO> UpdatePatientProfileAsync(int accountId, PatientProfileUpdateDTO profileDTO)
        {
            var basicDto = new BasicProfileUpdateDTO
            {
                Fullname = profileDTO.Fullname,
                Dob = profileDTO.Dob,
                Gender = profileDTO.Gender
            };

            var result = await UpdateBasicProfileAsync(accountId, basicDto);

            // Map BasicProfileResponseDTO to PatientProfileResponseDTO for backward compatibility
            return new PatientProfileResponseDTO
            {
                AccId = result.AccId,
                AccUsername = result.AccUsername,
                Email = result.Email,
                Fullname = result.Fullname,
                Dob = result.Dob,
                Gender = result.Gender,
                Roles = result.Roles,
                IsActive = result.IsActive
            };
        }

        // <summary>
        /// Updates doctor profile for Doctor (role=2)
        /// Allows updating: Fullname, Dob, Gender, Degree, Bio
        /// </summary>
        public async Task<DoctorProfileResponse> UpdateDoctorProfileAsync(int accountId, DoctorProfileUpdateDTO profileDTO)
        {
            if (profileDTO == null)
                throw new ArgumentNullException(nameof(profileDTO));

            var existingAccount = await _accountRepo.GetAccountByIdAsync(accountId);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Không tìm thấy tài khoản với id {accountId}.");

            if (existingAccount.Roles != 2)
                throw new UnauthorizedAccessException("Tài khoản này không phải là tài khoản bác sĩ.");

            // Validate Date of Birth if provided - FIXED
            if (profileDTO.Dob.HasValue)
            {
                var dobDateTime = profileDTO.Dob.Value.ToDateTime(TimeOnly.MinValue);
                ValidateDateOfBirth(dobDateTime, nameof(profileDTO.Dob));
            }

            // Create account object with only updated basic fields
            var accountToUpdate = new Account
            {
                AccId = accountId,
                AccUsername = existingAccount.AccUsername,
                AccPassword = existingAccount.AccPassword,
                Email = existingAccount.Email,
                Fullname = profileDTO.Fullname ?? existingAccount.Fullname,
                Dob = profileDTO.Dob ?? existingAccount.Dob,
                Gender = profileDTO.Gender ?? existingAccount.Gender,
                Roles = existingAccount.Roles,
                IsActive = existingAccount.IsActive
            };

            // Update account profile
            var updatedAccount = await _accountRepo.UpdateAccountProfileAsync(accountId, accountToUpdate);

            // Update doctor-specific information if provided
            var doctorRepo = new DoctorRepo();
            var doctor = await doctorRepo.GetDoctorByIdAsync(accountId);

            if (doctor != null && (!string.IsNullOrWhiteSpace(profileDTO.Degree) || !string.IsNullOrWhiteSpace(profileDTO.Bio)))
            {
                if (!string.IsNullOrWhiteSpace(profileDTO.Degree))
                    doctor.Degree = profileDTO.Degree;

                if (!string.IsNullOrWhiteSpace(profileDTO.Bio))
                    doctor.Bio = profileDTO.Bio;

                await doctorRepo.UpdateDoctorAsync(doctor.DctId, doctor);
            }

            // Get updated doctor information for response
            var updatedDoctor = await doctorRepo.GetDoctorByIdAsync(accountId);

            return new DoctorProfileResponse
            {
                AccId = updatedAccount.AccId,
                AccUsername = updatedAccount.AccUsername,
                Email = updatedAccount.Email,
                Fullname = updatedAccount.Fullname,
                Dob = updatedAccount.Dob,
                Gender = updatedAccount.Gender,
                Roles = updatedAccount.Roles,
                IsActive = updatedAccount.IsActive,
                Degree = updatedDoctor?.Degree,
                Bio = updatedDoctor?.Bio
            };
        }

        /// <summary>
        /// Updates staff profile for Staff (role=4)
        /// Allows updating: Fullname, Dob, Gender, Degree, Bio
        /// </summary>
        public async Task<StaffProfileResponseDTO> UpdateStaffProfileAsync(int accountId, StaffProfileUpdateDTO profileDTO)
        {
            if (profileDTO == null)
                throw new ArgumentNullException(nameof(profileDTO));

            var existingAccount = await _accountRepo.GetAccountByIdAsync(accountId);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Không tìm thấy tài khoản với id {accountId}.");

            if (existingAccount.Roles != 4)
                throw new UnauthorizedAccessException("Tài khoản này không phải là tài khoản nhân viên.");

            // Validate Date of Birth if provided - FIXED
            if (profileDTO.Dob.HasValue)
            {
                var dobDateTime = profileDTO.Dob.Value.ToDateTime(TimeOnly.MinValue);
                ValidateDateOfBirth(dobDateTime, nameof(profileDTO.Dob));
            }

            // Create account object with only updated basic fields
            var accountToUpdate = new Account
            {
                AccId = accountId,
                AccUsername = existingAccount.AccUsername,
                AccPassword = existingAccount.AccPassword,
                Email = existingAccount.Email,
                Fullname = profileDTO.Fullname ?? existingAccount.Fullname,
                Dob = profileDTO.Dob ?? existingAccount.Dob,
                Gender = profileDTO.Gender ?? existingAccount.Gender,
                Roles = existingAccount.Roles,
                IsActive = existingAccount.IsActive
            };

            // Update account profile
            var updatedAccount = await _accountRepo.UpdateAccountProfileAsync(accountId, accountToUpdate);

            // Update staff-specific information if provided
            var staffRepo = new StaffRepo();
            var staff = await staffRepo.GetStaffByIdAsync(accountId);

            if (staff != null && (!string.IsNullOrWhiteSpace(profileDTO.Degree) || !string.IsNullOrWhiteSpace(profileDTO.Bio)))
            {
                if (!string.IsNullOrWhiteSpace(profileDTO.Degree))
                    staff.Degree = profileDTO.Degree;

                if (!string.IsNullOrWhiteSpace(profileDTO.Bio))
                    staff.Bio = profileDTO.Bio;

                await staffRepo.UpdateStaffAsync(staff.StfId, staff);
            }

            // Get updated staff information for response
            var updatedStaff = await staffRepo.GetStaffByIdAsync(accountId);

            return new StaffProfileResponseDTO
            {
                AccId = updatedAccount.AccId,
                AccUsername = updatedAccount.AccUsername,
                Email = updatedAccount.Email,
                Fullname = updatedAccount.Fullname,
                Dob = updatedAccount.Dob,
                Gender = updatedAccount.Gender,
                Roles = updatedAccount.Roles,
                IsActive = updatedAccount.IsActive,
                Degree = updatedStaff?.Degree,
                Bio = updatedStaff?.Bio
            };
        }

        /// <summary>
        /// Universal profile update method that routes to appropriate profile update based on account role
        /// </summary>
        public async Task<object> UpdatePersonalProfileAsync(int accountId, object profileDTO)
        {
            if (profileDTO == null)
                throw new ArgumentNullException(nameof(profileDTO));

            // Get account to determine role
            var account = await _accountRepo.GetAccountByIdAsync(accountId);
            if (account == null)
                throw new KeyNotFoundException($"Không tìm thấy tài khoản với id {accountId}.");

            // Route to appropriate profile update based on role
            switch (account.Roles)
            {
                case 1: // Admin
                case 3: // Patient
                case 5: // Manager
                    // Convert to BasicProfileUpdateDTO
                    if (profileDTO is BasicProfileUpdateDTO basicDto)
                    {
                        return await UpdateBasicProfileAsync(accountId, basicDto);
                    }
                    // If the DTO is a different type, try to map the common fields
                    else if (profileDTO is DoctorProfileUpdateDTO doctorDto)
                    {
                        var basicFromDoctor = new BasicProfileUpdateDTO
                        {
                            Fullname = doctorDto.Fullname,
                            Dob = doctorDto.Dob,
                            Gender = doctorDto.Gender
                        };
                        return await UpdateBasicProfileAsync(accountId, basicFromDoctor);
                    }
                    else if (profileDTO is StaffProfileUpdateDTO staffDto)
                    {
                        var basicFromStaff = new BasicProfileUpdateDTO
                        {
                            Fullname = staffDto.Fullname,
                            Dob = staffDto.Dob,
                            Gender = staffDto.Gender
                        };
                        return await UpdateBasicProfileAsync(accountId, basicFromStaff);
                    }
                    else
                    {
                        throw new ArgumentException("Loại DTO hồ sơ không hợp lệ cho vai trò tài khoản này.");
                    }

                case 2: // Doctor
                    if (profileDTO is DoctorProfileUpdateDTO doctorUpdateDto)
                    {
                        return await UpdateDoctorProfileAsync(accountId, doctorUpdateDto);
                    }
                    else
                    {
                        throw new ArgumentException("Cập nhật hồ sơ bác sĩ yêu cầu DoctorProfileUpdateDTO.");
                    }

                case 4: // Staff
                    if (profileDTO is StaffProfileUpdateDTO staffUpdateDto)
                    {
                        return await UpdateStaffProfileAsync(accountId, staffUpdateDto);
                    }
                    else
                    {
                        throw new ArgumentException("Cập nhật hồ sơ nhân viên yêu cầu StaffProfileUpdateDTO.");
                    }

                default:
                    throw new UnauthorizedAccessException($"Cập nhật hồ sơ không được hỗ trợ cho vai trò {account.Roles}.");
            }
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
                throw new ArgumentException("Email là bắt buộc.");

            ValidateEmail(request.Email);

            var account = await _accountRepo.GetAccountByEmailAsync(request.Email);
            if (account == null)
                throw new InvalidOperationException("Không tìm thấy tài khoản với email được cung cấp.");

            var verificationCode = _verificationService.GenerateCode(request.Email);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(PENDING_REGISTRATION_EXPIRY_MINUTES))
                .SetSize(32); // Size for a string (verification code, e.g., 6-8 chars)
            _memoryCache.Set($"password_reset_{request.Email}", verificationCode, cacheOptions);

            return verificationCode; // Consider not returning the code directly
        }

        public async Task<(string verificationCode, string email)> InitiatePatientRegistrationAsync(PatientAccountRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.AccUsername))
                throw new ArgumentException("Tên người dùng là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.AccPassword))
                throw new ArgumentException("Mật khẩu là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email là bắt buộc.");

            // Use new async validation methods with duplicate checking
            await ValidateUsernameAsync(request.AccUsername, request.Email);
            ValidatePassword(request.AccPassword, request.AccUsername);
            await ValidateEmailAsync(request.Email, request.AccUsername);

            // Validate Date of Birth - FIXED
            if (request.Dob.HasValue)
            {
                ValidateDateOfBirth(request.Dob.Value, nameof(request.Dob));
            }
            else
            {
                throw new ArgumentException("Ngày sinh là bắt buộc để đăng ký bệnh nhân.", nameof(request.Dob));
            }

            // Store the registration request in cache
            var pendingRegistration = new PendingPatientRegistrationDTO
            {
                AccUsername = request.AccUsername,
                AccPassword = request.AccPassword, // Consider hashing this for security
                Email = request.Email,
                Fullname = request.Fullname,
                Dob = request.Dob,
                Gender = request.Gender
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(PENDING_REGISTRATION_EXPIRY_MINUTES))
                .SetSize(1024); // Specify size

            var verificationCode = _verificationService.GenerateCode(request.Email);
            _memoryCache.Set($"pending_registration_{request.Email}", pendingRegistration, cacheOptions);

            return (verificationCode, request.Email);
        }

        public async Task<AccountResponseDTO> VerifyEmailAndCreateAccountAsync(string email, string code)
        {
            if (!_verificationService.VerifyCode(email, code))
                throw new InvalidOperationException("Mã xác minh không hợp lệ hoặc đã hết hạn.");

            // Validate email
            ValidateEmail(email);

            // Retrieve pending registration
            var cacheKey = $"pending_registration_{email}";
            if (!_memoryCache.TryGetValue(cacheKey, out PendingPatientRegistrationDTO? pendingRegistration))
                throw new InvalidOperationException("Không tìm thấy đăng ký đang chờ hoặc đăng ký đã hết hạn.");

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

            return patientResponse.Account ?? throw new InvalidOperationException("Không thể tạo tài khoản.");
        }

        public async Task<(bool isValid, string message)> HasPendingRegistrationAsync(string email)
        {
            // Validate email
            ValidateEmail(email);

            var pendingRegistration = _memoryCache.Get<PendingPatientRegistrationDTO>($"pending_registration_{email}");

            if (pendingRegistration == null)
                return (false, "Không tìm thấy đăng ký đang chờ cho email này.");

            // Check if there's already an account with this email
            var existingAccount = await GetAccountByEmailAsync(email);
            if (existingAccount != null)
            {
                if (existingAccount.IsActive == true)
                    return (false, "Tài khoản đã được xác minh.");
                return (true, ""); // Account exists but not verified
            }

            return (true, ""); // Pending registration exists
        }

        public async Task<bool> ChangePasswordAsync(int accId, ChangePasswordRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.currentPassword))
                throw new ArgumentException("Mật khẩu hiện tại là bắt buộc.", nameof(request.currentPassword));
            if (string.IsNullOrWhiteSpace(request.newPassword))
                throw new ArgumentException("Mật khẩu mới là bắt buộc.", nameof(request.newPassword));
            if (string.IsNullOrWhiteSpace(request.confirmNewPassword))
                throw new ArgumentException("Xác nhận mật khẩu mới là bắt buộc.", nameof(request.confirmNewPassword));

            if (request.newPassword != request.confirmNewPassword)
                throw new InvalidOperationException("Mật khẩu mới và xác nhận không khớp.");

            var account = await _accountRepo.GetAccountByIdAsync(accId);
            if (account == null)
                throw new KeyNotFoundException($"Không tìm thấy tài khoản với id {accId}.");

            // Verify the current password using the hashed password in database
            if (!VerifyPassword(request.currentPassword, account.AccPassword))
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");

            // Validate new password
            ValidatePassword(request.newPassword, account.AccUsername);

            // Check if new password is different from current password
            if (VerifyPassword(request.newPassword, account.AccPassword))
                throw new InvalidOperationException("Mật khẩu mới phải khác với mật khẩu hiện tại.");

            // Update password
            var changeRequest = new ChangePasswordRequestDTO
            {
                currentPassword = request.currentPassword,
                newPassword = HashPassword(request.newPassword),
                confirmNewPassword = HashPassword(request.confirmNewPassword)
            };

            var result = await _accountRepo.ChangePasswordAsync(accId, changeRequest);
            return result;
        }

        public async Task<(bool Success, string Message)> SendPasswordResetCodeAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email là bắt buộc.");

            try
            {
                // Validate email format
                ValidateEmail(email);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }

            // Check if account exists for the email
            var account = await _accountRepo.GetAccountByEmailAsync(email);
            if (account == null)
                return (false, "Không tìm thấy tài khoản với email được cung cấp.");

            // Check if account is active
            if (account.IsActive == false)
                return (false, "Tài khoản không hoạt động.");

            // Generate verification code
            var code = _verificationService.GenerateCode(email);

            // Store the code in cache with expiry - using consistent cache key
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(PENDING_REGISTRATION_EXPIRY_MINUTES))
                .SetSize(32);
            _memoryCache.Set($"password_reset_{email}", code, cacheOptions);

            // TODO: Send code to user's email (email sending not implemented here)
            return (true, "Mã đặt lại mật khẩu đã được gửi đến email của bạn.");
        }

        public async Task<(bool Success, string Message)> VerifyPasswordResetCodeAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email là bắt buộc.");
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Mã xác minh là bắt buộc.");

            try
            {
                // Validate email format
                ValidateEmail(email);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }

            // Check if the code exists in cache - using consistent cache key
            var cacheKey = $"password_reset_{email}";
            if (!_memoryCache.TryGetValue<string>(cacheKey, out var cachedCode))
                return (false, "Không tìm thấy yêu cầu đặt lại mật khẩu hoặc mã đã hết hạn.");

            if (!string.Equals(cachedCode, code, StringComparison.Ordinal))
                return (false, "Mã xác minh không hợp lệ.");

            // Don't remove the code here - let it be removed in ResetPasswordAsync
            // This allows multiple verification attempts before actual reset
            return (true, "Mã xác minh hợp lệ.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email là bắt buộc.");
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Mã xác minh là bắt buộc.");
            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "Mật khẩu mới là bắt buộc.");

            try
            {
                // Validate email format
                ValidateEmail(email);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }

            var account = await _accountRepo.GetAccountByEmailAsync(email);
            if (account == null)
                return (false, "Không tìm thấy tài khoản với email được cung cấp.");

            if (account.IsActive == false)
                return (false, "Tài khoản không hoạt động.");

            // Verify the code first
            var cacheKey = $"password_reset_{email}";
            if (!_memoryCache.TryGetValue<string>(cacheKey, out var cachedCode))
                return (false, "Không tìm thấy yêu cầu đặt lại mật khẩu hoặc mã đã hết hạn.");

            if (!string.Equals(cachedCode, code, StringComparison.Ordinal))
                return (false, "Mã xác minh không hợp lệ.");

            try
            {
                // Validate new password
                ValidatePassword(newPassword, account.AccUsername);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }

            // Check if new password is different from current password
            if (VerifyPassword(newPassword, account.AccPassword))
                return (false, "Mật khẩu mới phải khác với mật khẩu hiện tại.");

            // Hash the new password
            var hashedNewPassword = HashPassword(newPassword);

            // Update the password directly in the repository
            var changeRequest = new ChangePasswordRequestDTO
            {
                currentPassword = account.AccPassword,
                newPassword = hashedNewPassword,
                confirmNewPassword = hashedNewPassword
            };

            var result = await _accountRepo.ChangePasswordAsync(account.AccId, changeRequest);
            if (!result)
                return (false, "Không thể đặt lại mật khẩu.");

            // Remove the verification code from cache after successful reset
            _memoryCache.Remove(cacheKey);

            return (true, "Mật khẩu đã được đặt lại thành công.");
        }
    }
}