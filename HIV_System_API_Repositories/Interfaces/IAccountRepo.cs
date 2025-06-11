using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IAccountRepo
    {
        Task<List<Account>> GetAllAccountsAsync();
        Task<Account> GetAccountByLoginAsync(string accUsername, string accPassword);
        Task<Account?> GetAccountByIdAsync(int accId);
        Task<Account?> GetAccountByUsernameAsync(string accUsername);
        Task<bool> UpdateAccountByIdAsync(int id);
        Task<bool> DeleteAccountAsync(int accId);
        Task<bool> CreateAccountAsync(Account account);
    }
}
