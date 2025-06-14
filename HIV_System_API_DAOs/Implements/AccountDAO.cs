using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.AccountDTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class AccountDAO : IAccountDAO
    {
        private readonly HivSystemApiContext _context;
        private static AccountDAO? _instance;

        public AccountDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static AccountDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AccountDAO();
                }
                return _instance;
            }
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account?> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccUsername == accUsername && a.AccPassword == accPassword);
        }

        public async Task<Account?> GetAccountByIdAsync(int accId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == accId);
        }

        public async Task<Account> UpdateAccountByIdAsync(int id, Account updatedAccount)
        {
            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with id {id} not found.");
            }

            // Update fields
            existingAccount.AccPassword = updatedAccount.AccPassword;
            existingAccount.Email = updatedAccount.Email;
            existingAccount.Fullname = updatedAccount.Fullname;
            existingAccount.Dob = updatedAccount.Dob;
            existingAccount.Gender = updatedAccount.Gender;
            existingAccount.Roles = updatedAccount.Roles;
            existingAccount.IsActive = updatedAccount.IsActive;

            // Note: Navigation properties (Doctor, Patient, Staff) are not updated here.

            await _context.SaveChangesAsync();
            return existingAccount;
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == accId);
            if (account == null)
            {
                return false;
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }
    }
}
