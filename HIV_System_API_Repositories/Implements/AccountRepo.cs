using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class AccountRepo : IAccountRepo
    {
        public async Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account)
        {
            return await AccountDAO.Instance.CreateAccountAsync(account);
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            return await AccountDAO.Instance.DeleteAccountAsync(accId);
        }

        public async Task<AccountResponseDTO?> GetAccountByIdAsync(int accId)
        {
            return await AccountDAO.Instance.GetAccountByIdAsync(accId);
        }

        public async Task<AccountResponseDTO> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            return await AccountDAO.Instance.GetAccountByLoginAsync(accUsername, accPassword);
        }

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync()
        {
            return await AccountDAO.Instance.GetAllAccountsAsync();
        }

        public async Task<bool> UpdateAccountByIdAsync(int id, AccountRequestDTO updatedAccount)
        {
            return await AccountDAO.Instance.UpdateAccountByIdAsync(id, updatedAccount);
        }
    }
}
