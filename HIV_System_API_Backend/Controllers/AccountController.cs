using Azure;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountService _accountService;
        private readonly IConfiguration _configuration;
        public AccountController( IConfiguration configuration)
        {
            _configuration = configuration;
            _accountService = new AccountService();
        }

        [HttpGet("GetAllAccounts")]
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
            
            var tokenString = GenerateJSONWebToken(account);
            return Ok(new { 
                token = tokenString,
                username = account.AccUsername,
                email = account.Email,
                role = account.Roles,
                accountId = account.AccId
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
        [Authorize(Roles ="1")]
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
                return Conflict($"Account creation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpPut("UpdateAccount/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountRequestDTO accountDTO)
        {
            if (accountDTO == null ||
                string.IsNullOrWhiteSpace(accountDTO.AccUsername) ||
                string.IsNullOrWhiteSpace(accountDTO.AccPassword))
            {
                return BadRequest("Username and password are required.");
            }

            try
            {
                var updatedAccount = await _accountService.UpdateAccountByIdAsync(id, accountDTO);
                if (updatedAccount == null)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return Ok(updatedAccount);
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Account update failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("DeleteAccount/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var deleted = await _accountService.DeleteAccountAsync(id);
                if (!deleted)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Account deletion failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("CreatePatientAccount")]
        public async Task<IActionResult> CreatePatientAccount([FromBody] PatientAccountRequestDTO request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.AccUsername) ||
                string.IsNullOrWhiteSpace(request.AccPassword))
            {
                return BadRequest("Username and password are required.");
            }

            try
            {
                var createdPatient = await _accountService.CreatePatientAccountAsync(request);
                if (createdPatient == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Patient account could not be created.");
                }
                return CreatedAtAction(nameof(GetAccountById), new { id = createdPatient.AccId }, createdPatient);
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Patient account creation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
