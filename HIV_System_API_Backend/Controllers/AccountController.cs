using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
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
                if (accounts == null || !accounts.Any())
                {
                    return NotFound("No accounts found.");
                }
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

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRequestDTO accountDTO)
        {
            try
            {
                if (accountDTO == null)
                {
                    return BadRequest("Account data cannot be null.");
                }
                var createdAccount = await _accountService.CreateAccountAsync(accountDTO);
                if (createdAccount == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create account.");
                }
                return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.AccId }, createdAccount);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Source}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpPut("UpdateAccount/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountRequestDTO accountDTO)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Account ID must be greater than zero.");
                }
                if (accountDTO == null)
                {
                    return BadRequest("Account data cannot be null.");
                }
                var result = await _accountService.UpdateAccountByIdAsync(id, accountDTO);
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
