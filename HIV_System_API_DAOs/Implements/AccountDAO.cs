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

        public async Task<bool> UpdateAccountByIdAsync(int id)
        {
            // Validate input parameter
            if (id <= 0)
            {
                throw new ArgumentException("Account ID must be greater than zero.");
            }

            // Find the account by ID
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                // Account not found
                return false;
            }

            // No data changes, just save to ensure method contract
            _context.Accounts.Update(account);
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

        public async Task<bool> CreateAccountAsync(Account account)
        {
            // Validate input parameter
            if (account == null)
            {
                throw new ArgumentException("Account object cannot be null.");
            }

            // Ensure the account ID is not set (let the database generate it)
            account.AccId = 0;
            account.Roles = 5;

            // Add the account to the database
            await _context.Accounts.AddAsync(account);

            // Save changes to the database
            var result = await _context.SaveChangesAsync();

            // Return true if at least one record was added, otherwise false
            return result > 0;
        }
    }
}
