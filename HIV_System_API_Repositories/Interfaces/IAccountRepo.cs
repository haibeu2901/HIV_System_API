using HIV_System_API_BOs;
using HIV_System_API_DTOs;
using HIV_System_API_DTOs.AccountDTO;
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
        Task<Account?> GetAccountByLoginAsync(string accUsername, string accPassword);
        Task<Account?> GetAccountByIdAsync(int accId);
        Task<Account> UpdateAccountByIdAsync(int id, Account updatedAccount);
        Task<bool> DeleteAccountAsync(int accId);
        Task<Account> CreateAccountAsync(Account account);
        Task<Patient> CreatePatientAccountAsync(Patient patient);
        Task<bool> IsEmailUsedAsync(string mail);
    }
}
