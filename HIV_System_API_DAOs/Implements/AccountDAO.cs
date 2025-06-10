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
    }
}
