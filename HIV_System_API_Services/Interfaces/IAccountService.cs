using HIV_System_API_BOs;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IAccountService
    {
        Task<List<AccountDTO>> GetAllAccountsAsync();
        Task<AccountDTO?> GetAccountByLoginAsync(string accUsername, string accPassword);
        Task<AccountDTO?> GetAccountByIdAsync(int accId);
        Task<AccountDTO?> GetAccountByUsernameAsync(string accUsername);
        Task<bool> UpdateAccountByIdAsync(int id, AccountDTO accountDTO);
        Task<bool> DeleteAccountAsync(int accId);
        Task<AccountDTO> CreateAccountAsync(AccountDTO accountDTO);
    }
}
