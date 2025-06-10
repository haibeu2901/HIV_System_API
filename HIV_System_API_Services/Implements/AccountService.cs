using HIV_System_API_BOs;
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

        public async Task<Account> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            return await _accountRepo.GetAccountByLoginAsync(accUsername, accPassword);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _accountRepo.GetAllAccountsAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int accId)
        {
            return await _accountRepo.GetAccountByIdAsync(accId);
        }

        public async Task<Account?> GetAccountByUserameAsync(string accUsername)
        {
            return await _accountRepo.GetAccountByUsernameAsync(accUsername);
        }

        public async Task<bool> UpdateAccountByIdAsync(int id)
        {
            return await _accountRepo.UpdateAccountByIdAsync(id);
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            return await _accountRepo.DeleteAccountAsync(accId);
        }

        public async Task<bool> CreateAccountAsync(Account account)
        {
            return await _accountRepo.CreateAccountAsync(account);
        }
    }
}
