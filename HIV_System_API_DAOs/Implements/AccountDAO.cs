using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
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

        public async Task<Account> GetAccountByLoginAsync(string accUsername, string accPassword)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(accUsername) || string.IsNullOrWhiteSpace(accPassword))
            {
                throw new ArgumentException("Username and password cannot be null or empty.");
            }

            // Query the database asynchronously to find the account
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccUsername == accUsername && a.AccPassword == accPassword);

            // Return the account or null if not found
            return account;
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int accId)
        {
            // Validate input parameter
            if (accId <= 0)
            {
                throw new ArgumentException("Account ID must be greater than zero.");
            }
            // Query the database asynchronously to find the account by ID
            var account = await _context.Accounts.FindAsync(accId);
            // Return the account or null if not found
            return account;
        }

        public async Task<Account?> GetAccountByUsernameAsync(string accUsername)
        {
            if (string.IsNullOrWhiteSpace(accUsername))
            {
                throw new ArgumentException("Username cannot be null or empty.");
            }

            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccUsername.ToLower().Contains(accUsername.ToLower()));
        }

        public async Task<bool> UpdateAccountByIdAsync(int id, Account updatedAccount)
        {
            if (id <= 0)
                throw new ArgumentException("ID tài khoản phải lớn hơn 0.");

            if (updatedAccount == null)
                throw new ArgumentException("Thông tin tài khoản không được để trống.");

            var existingAccount = await _context.Accounts.FindAsync(id);
            if (existingAccount == null)
                return false;

            // Update fields
            existingAccount.AccUsername = updatedAccount.AccUsername ?? existingAccount.AccUsername;
            existingAccount.AccPassword = updatedAccount.AccPassword ?? existingAccount.AccPassword;
            existingAccount.Email = updatedAccount.Email ?? existingAccount.Email;
            existingAccount.Fullname = updatedAccount.Fullname ?? existingAccount.Fullname;
            existingAccount.Dob = updatedAccount.Dob ?? existingAccount.Dob;
            existingAccount.Gender = updatedAccount.Gender ?? existingAccount.Gender;
            existingAccount.Roles = updatedAccount.Roles != 0 ? updatedAccount.Roles : existingAccount.Roles;
            existingAccount.IsActive = updatedAccount.IsActive;

            // Validate updated data
            if (existingAccount.AccUsername.Length > 100)
                throw new ArgumentException("Tên đăng nhập phải dưới 100 ký tự.");

            if (existingAccount.Email != null && existingAccount.Email.Length > 100)
                throw new ArgumentException("Email phải dưới 100 ký tự.");

            if (existingAccount.Fullname != null && existingAccount.Fullname.Length > 50)
                throw new ArgumentException("Họ tên phải dưới 50 ký tự.");

            if (existingAccount.Roles < 1 || existingAccount.Roles > 5)
                throw new ArgumentException("Vai trò phải từ 1 đến 5.");

            _context.Accounts.Update(existingAccount);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAccountAsync(int accId)
        {
            // Validate input parameter
            if (accId <= 0)
            {
                throw new ArgumentException("Account ID must be greater than zero.");
            }

            // Find the account by ID
            var account = await _context.Accounts.FindAsync(accId);
            if (account == null)
            {
                // Account not found, return false
                return false;
            }

            // Remove the account
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            // Return true to indicate successful deletion
            return true;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            // Validate input
            if (account == null)
                throw new ArgumentException("Tài khoản không được để trống.");

            if (string.IsNullOrWhiteSpace(account.AccUsername) || account.AccUsername.Length > 100)
                throw new ArgumentException("Tên đăng nhập là bắt buộc và phải dưới 100 ký tự.");

            if (string.IsNullOrWhiteSpace(account.AccPassword))
                throw new ArgumentException("Mật khẩu là bắt buộc.");

            if (account.Email != null && account.Email.Length > 100)
                throw new ArgumentException("Email phải dưới 100 ký tự.");

            if (account.Fullname != null && account.Fullname.Length > 50)
                throw new ArgumentException("Họ tên phải dưới 50 ký tự.");

            if (account.Roles < 1 || account.Roles > 5)
                throw new ArgumentException("Vai trò phải từ 1 đến 5.");

            // Hash password
            account.AccId = 0; // Ensure ID is generated by database

            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return account;
        }
    }
}
