using HIV_System_API_BOs;
using HIV_System_API_DTOs;
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

        private AccountDTO ToDTO(Account account)
        {
            return new AccountDTO
            {
                AccId = account.AccId,
                AccUsername = account.AccUsername,
                AccPassword = account.AccPassword,
                Fullname = account.Fullname,
                Email = account.Email,
                Dob = account.Dob,
                Gender = account.Gender,
                Roles = account.Roles,
                IsActive = account.IsActive
            };
        }

        private Account ToEntity(AccountDTO accountDTO)
        {
            return new Account
            {
                AccId = accountDTO.AccId,
                AccUsername = accountDTO.AccUsername,
                AccPassword = accountDTO.AccPassword,
                Fullname = accountDTO.Fullname,
                Email = accountDTO.Email,
                Dob = accountDTO.Dob,
                Gender = accountDTO.Gender,
                Roles = accountDTO.Roles,
                IsActive = accountDTO.IsActive
            };
        }

        public async Task<AccountDTO> CreateAccountAsync(AccountDTO accountDTO)
        {
            var account = ToEntity(accountDTO);
            var createdAccount = await _accountRepo.CreateAccountAsync(account);
            if (createdAccount == null)
            {
                throw new Exception("Failed to create account.");
            }
            return ToDTO(createdAccount);
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            return await _accountRepo.DeleteAccountAsync(accId);
        }

        public async Task<AccountDTO?> GetAccountByIdAsync(int accId)
        {
            var account = await _accountRepo.GetAccountByIdAsync(accId);
            if (account == null)
            {
                return null;
            }
            return ToDTO(account);
        }

        public async Task<AccountDTO?> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            var account = await _accountRepo.GetAccountByLoginAsync(accUsername, accPassword);
            if (account == null)
            {
                return null;
            }
            return ToDTO(account);
        }

        public async Task<AccountDTO?> GetAccountByUsernameAsync(string accUsername)
        {
            var account = await _accountRepo.GetAccountByUsernameAsync(accUsername);
            if (account == null)
            {
                return null;
            }
            return ToDTO(account);
        }

        public async Task<List<AccountDTO>> GetAllAccountsAsync()
        {
            var accounts = await _accountRepo.GetAllAccountsAsync();
            return accounts.Select(ToDTO).ToList();
        }

        public Task<bool> UpdateAccountByIdAsync(int id, AccountDTO accountDTO)
        {
            var account = ToEntity(accountDTO);
            return _accountRepo.UpdateAccountByIdAsync(id, account);
        }
    }
}
