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
        private readonly HivSystemContext _context;
        private static AccountDAO _instance;

        public AccountDAO()
        {
            _context = new HivSystemContext();
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

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync()
        {
            var accounts = await _context.Accounts.ToListAsync();
            var result = accounts.Select(a => new AccountResponseDTO
            {
                AccId = a.AccId,
                AccUsername = a.AccUsername,
                AccPassword = a.AccPassword,
                Email = a.Email,
                Fullname = a.Fullname,
                Dob = a.Dob,
                Gender = a.Gender,
                Roles = a.Roles,
                IsActive = a.IsActive
            }).ToList();
            return result;
        }

        public async Task<AccountResponseDTO> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccUsername == accUsername && a.AccPassword == accPassword);

            if (account == null)
            {
                return null;
            }

            return new AccountResponseDTO
            {
                AccId = account.AccId,
                AccUsername = account.AccUsername,
                AccPassword = account.AccPassword,
                Email = account.Email,
                Fullname = account.Fullname,
                Dob = account.Dob,
                Gender = account.Gender,
                Roles = account.Roles,
                IsActive = account.IsActive
            };
        }

        public async Task<AccountResponseDTO?> GetAccountByIdAsync(int accId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == accId);
            if (account == null)
            {
                return null;
            }

            return new AccountResponseDTO
            {
                AccId = account.AccId,
                AccUsername = account.AccUsername,
                AccPassword = account.AccPassword,
                Email = account.Email,
                Fullname = account.Fullname,
                Dob = account.Dob,
                Gender = account.Gender,
                Roles = account.Roles,
                IsActive = account.IsActive
            };
        }

        public async Task<bool> UpdateAccountByIdAsync(int id, AccountRequestDTO updatedAccount)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == id);
            if (account == null)
            {
                return false;
            }

            account.AccPassword = updatedAccount.AccPassword;
            account.Email = updatedAccount.Email;
            account.Fullname = updatedAccount.Fullname;
            account.Dob = updatedAccount.Dob;
            account.Gender = updatedAccount.Gender;
            account.Roles = updatedAccount.Roles;
            account.IsActive = updatedAccount.IsActive;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
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

        public async Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            var newAccount = new Account
            {
                AccUsername = account.AccUsername,
                AccPassword = account.AccPassword,
                Email = account.Email,
                Fullname = account.Fullname,
                Dob = account.Dob,
                Gender = account.Gender,
                Roles = account.Roles,
                IsActive = account.IsActive
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return new AccountResponseDTO
            {
                AccId = newAccount.AccId,
                AccUsername = account.AccUsername,
                AccPassword= account.AccPassword,
                Email = newAccount.Email,
                Fullname = newAccount.Fullname,
                Dob = newAccount.Dob,
                Gender = newAccount.Gender,
                Roles = newAccount.Roles,
                IsActive = newAccount.IsActive
            };
        }
    }
}
