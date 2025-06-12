using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IAccountDAO
    {
        Task<List<AccountResponseDTO>> GetAllAccountsAsync();
        Task<AccountResponseDTO> GetAccountByLoginAsync(string accUsername, string accPassword);
        Task<AccountResponseDTO?> GetAccountByIdAsync(int accId);
        Task<bool> UpdateAccountByIdAsync(int id, AccountRequestDTO updatedAccount);
        Task<bool> DeleteAccountAsync(int accId);
        Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account);
    }
}
