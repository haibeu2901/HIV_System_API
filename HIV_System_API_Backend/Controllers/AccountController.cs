using Azure;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.EmailDTO;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountService _accountService;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IVerificationCodeService _verificationService;

        public AccountController(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _accountService = new AccountService(memoryCache);
            _emailSender = new EmailSender(configuration);
            _verificationService = new VerificationCodeService(memoryCache);
        }

        [HttpGet("GetAllAccounts")]
        [Authorize(Roles = "1, 5")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            if (accounts == null || accounts.Count == 0)
            {
                return NotFound("No accounts found.");
            }
            return Ok(accounts);
        }

        [HttpPost("GetAccountByLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAccountByLogin([FromBody] LoginRequestDTO loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.AccUsername) || string.IsNullOrWhiteSpace(loginRequest.AccPassword))
            {
                return BadRequest("Username and password are required.");
            }

            var account = await _accountService.GetAccountByLoginAsync(loginRequest.AccUsername, loginRequest.AccPassword);
            if (account == null)
            {
                return NotFound("Account not found or invalid credentials.");
            }
            if (account.IsActive == false)
            {
                return Unauthorized("Please verify your email before logging in or contact support if you have verified.");
            }

            var tokenString = GenerateJSONWebToken(account);
            return Ok(new
            {
                token = tokenString,
                account = new AccountResponseDTO
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
            });
        }

        private string GenerateJSONWebToken(AccountResponseDTO account)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.AccUsername),
                new Claim(JwtRegisteredClaimNames.Email, account.Email ?? ""),
                new Claim("AccountId", account.AccId.ToString()),
                new Claim(ClaimTypes.Role, account.Roles.ToString()), // This will store the byte value
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("GetAccountById/{id}")]
        [Authorize(Roles = "1, 5")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound($"Account with ID {id} not found.");
            }
            return Ok(account);
        }

        [HttpPost("CreateAccount")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRequestDTO accountDTO)
        {
            if (accountDTO == null ||
                string.IsNullOrWhiteSpace(accountDTO.AccUsername) ||
                string.IsNullOrWhiteSpace(accountDTO.AccPassword))
            {
                return BadRequest("Username and password are required.");
            }

            try
            {
                var createdAccount = await _accountService.CreateAccountAsync(accountDTO);
                if (createdAccount == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Account could not be created.");
                }
                return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.AccId }, createdAccount);
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Account creation failed: {ex.InnerException}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }
        
        [HttpPut("UpdateAccount/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountRequestDTO accountDTO)
        {
            try
            {
                //get account roles
                var currrentAccountRoles = byte.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");
                var currentAccountId = int.Parse(User.FindFirst("AccountId")?.Value ?? "0");

                var updatedAccount = await _accountService.UpdateAccountByIdAsync(id, accountDTO);
                if (updatedAccount == null)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return Ok(updatedAccount);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Account update failed: {ex.InnerException}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }

        [HttpDelete("DeleteAccount/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var currentUserRole = byte.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");
                var currentUserId = int.Parse(User.FindFirst("AccountId")?.Value ?? "0");

                var deleted = await _accountService.DeleteAccountAsync(id);
                if (!deleted)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return Ok("Account has been successfully deleted.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }

        [HttpPost("RegisterPatient")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientAccountRequestDTO request)
        {
            if (request == null)
                return BadRequest("Registration details are required.");

            try
            {
                var (verificationCode, email) = await _accountService.InitiatePatientRegistrationAsync(request);

                // Send verification email
                await SendVerificationEmail(email, verificationCode);

                return Ok(new { message = "Registration initiated. Please check your email for verification code." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPut("UpdatePatientProfile")]
        [Authorize(Roles = "3")] // Patient role
        public async Task<IActionResult> UpdatePatientProfile([FromBody] PatientProfileUpdateDTO profileDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("AccountId")?.Value ?? "0");

                if (currentUserId == 0)
                {
                    return Unauthorized("Invalid user session.");
                }

                var updatedProfile = await _accountService.UpdatePatientProfileAsync(currentUserId, profileDTO);
                return Ok(updatedProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("UpdateDoctorProfile")]
        [Authorize(Roles = "2")] // Doctor role
        public async Task<IActionResult> UpdateDoctorProfile([FromBody] DoctorProfileUpdateDTO profileDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("AccountId")?.Value ?? "0");

                if (currentUserId == 0)
                {
                    return Unauthorized("Invalid user session.");
                }

                var updatedProfile = await _accountService.UpdateDoctorProfileAsync(currentUserId, profileDTO);
                return Ok(updatedProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                    return BadRequest(new { message = "Email is required" });

                // Verify if there's a pending registration by checking with account service
                var (isValid, message) = await _accountService.HasPendingRegistrationAsync(request.Email);
                if (!isValid)
                    return BadRequest(new { message });

                // Generate new verification code
                var code = _verificationService.GenerateCode(request.Email);
                await SendVerificationEmail(request.Email, code);

                return Ok(new { message = "New verification code has been sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("verify-registration")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyRegistration([FromBody] EmailVerificationRequest request)
        {
            try
            {
                var account = await _accountService.VerifyEmailAndCreateAccountAsync(request.Email, request.Code);

                // If we got here, the account was created successfully
                // Generate JWT token for immediate login
                var token = GenerateJSONWebToken(account);

                // Create a simplified response object to avoid serialization issues
                var response = new
                {
                    message = "Account created successfully",
                    token = token,
                    account = new
                    {
                        account.AccId,
                        account.AccUsername,
                        account.Email,
                        account.Fullname,
                        account.Dob,
                        account.Gender,
                        account.Roles,
                        account.IsActive
                    }
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Even if account creation succeeded, there might be an issue with response generation
                // Log the error but still return success if we can verify the account exists
                try
                {
                    var createdAccount = await _accountService.GetAccountByEmailAsync(request.Email);
                    if (createdAccount != null && createdAccount.IsActive == true)
                    {
                        var token = GenerateJSONWebToken(createdAccount);
                        var response = new
                        {
                            message = "Account created successfully",
                            token = token,
                            account = new
                            {
                                createdAccount.AccId,
                                createdAccount.AccUsername,
                                createdAccount.Email,
                                createdAccount.Fullname,
                                createdAccount.Dob,
                                createdAccount.Gender,
                                createdAccount.Roles,
                                createdAccount.IsActive
                            }
                        };
                        return Ok(response);
                    }
                }
                catch
                {
                    // If the fallback fails, then return the 500 error
                }

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Account was created but there was an error generating the response." });
            }
        }

        private async Task SendVerificationEmail(string email, string code)
        {
            var emailContent = $@"
            <h2>Welcome to HIV System</h2>
            <p>Your verification code is: <strong>{code}</strong></p>
            <p>This code will expire in 4 minutes.</p>
            <p>If the code expires, you can request a new one through the application.</p>";

            await _emailSender.SendEmailAsync(
                email,
                "Verify Your Email",
                emailContent
            );
        }
    }
}
