using HIV_System_API_BOs;
using HIV_System_API_DTOs;
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
            var accounts = await _accountService.GetAllAccountsAsync();
            if (accounts == null || accounts.Count == 0)
            {
                return NotFound("No accounts found.");
            }
            return Ok(accounts);
        }

        [HttpPost("GetAccountByLogin")]
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
            return Ok(account);
        }

        [HttpGet("GetAccountById/{id}")]
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
    }
}
