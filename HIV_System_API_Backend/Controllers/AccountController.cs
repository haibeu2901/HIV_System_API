using HIV_System_API_BOs;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountService _accountService;
        public AccountController()
        {
            _accountService = new AccountService();
        }

        [HttpGet("GetAllAccounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var accounts = await _accountService.GetAllAccountsAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAccountByLogin/{username}/{password}")]
        public async Task<IActionResult> GetAccountByLogin(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest("Username and password cannot be null or empty.");
                }
                var account = await _accountService.GetAccountByLoginAsync(username, password);
                if (account == null)
                {
                    return NotFound($"Account with username {username} not found.");
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAccountById/{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Account ID must be greater than zero.");
                }
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAccountByUsername/{username}")]
        public async Task<IActionResult> GetAccountByUsername(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest("Username cannot be null or empty.");
                }
                var account = await _accountService.GetAccountByUserameAsync(username);
                if (account == null)
                {
                    return NotFound($"Account with username {username} not found.");
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            try
            {
                if (account == null)
                {
                    return BadRequest("Account cannot be null.");
                }
                var result = await _accountService.CreateAccountAsync(account);
                if (result)
                {
                    return CreatedAtAction(nameof(GetAccountById), new { id = account.AccId }, account);
                }
                return BadRequest("Failed to create account.");
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpPut("UpdateAccount/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] Account updatedAccount)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Account ID must be greater than zero.");
                }
                if (updatedAccount == null)
                {
                    return BadRequest("Updated account data cannot be null.");
                }

                var existingAccount = await _accountService.GetAccountByIdAsync(id);
                if (existingAccount == null)
                {
                    return NotFound($"Account with ID {id} not found.");
                }

                // Update properties
                existingAccount.AccId = id;
                existingAccount.AccUsername = updatedAccount.AccUsername;
                existingAccount.AccPassword = updatedAccount.AccPassword;
                existingAccount.Email = updatedAccount.Email;
                existingAccount.Fullname = updatedAccount.Fullname;
                existingAccount.Dob = updatedAccount.Dob;
                existingAccount.Gender = updatedAccount.Gender;
                existingAccount.Roles = updatedAccount.Roles;
                existingAccount.IsActive = updatedAccount.IsActive;

                var result = await _accountService.UpdateAccountByIdAsync(id);
                if (result)
                {
                    return NoContent();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update account.");
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteAccount/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Account ID must be greater than zero.");
                }
                var result = await _accountService.DeleteAccountAsync(id);
                if (result)
                {
                    return NoContent(); // 204 No Content
                }
                return NotFound($"Account with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
