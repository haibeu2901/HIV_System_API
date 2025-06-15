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
        public async Task<Account> CreateAccountAsync(Account account)
        {
            return await AccountDAO.Instance.CreateAccountAsync(account);
        }

        public async Task<Patient> CreatePatientAccountAsync(Patient patient)
        {
            return await AccountDAO.Instance.CreatePatientAccountAsync(patient);
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            return await AccountDAO.Instance.DeleteAccountAsync(accId);
        }

        public async Task<Account?> GetAccountByIdAsync(int accId)
        {
            return await AccountDAO.Instance.GetAccountByIdAsync(accId);
        }

        public async Task<Account?> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            return await AccountDAO.Instance.GetAccountByLoginAsync(accUsername, accPassword);
        }

        public async Task<Account?> GetAccountByUsernameAsync(string username)
        {
            return await AccountDAO.Instance.GetAccountByUsernameAsync(username);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await AccountDAO.Instance.GetAllAccountsAsync();
        }

        public async Task<bool> IsEmailUsedAsync(string mail)
        {
            return await AccountDAO.Instance.IsEmailUsedAsync(mail);
        }

        public async Task<Account> UpdateAccountByIdAsync(int id, Account updatedAccount)
        {
            return await AccountDAO.Instance.UpdateAccountByIdAsync(id, updatedAccount);
        }

        public Task<Account> UpdateAccountProfileAsync(int id, Account updatedAccount)
        {
            return AccountDAO.Instance.UpdateAccountProfileAsync(id, updatedAccount);
        }
    }
}
