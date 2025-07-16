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

        // Use for unit testing or dependency injection
        public AccountDAO(HivSystemApiContext context)
        {
            _context = context;
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
            var account = await _context.Accounts
                .SingleOrDefaultAsync(a => (a.AccUsername == accUsername || a.Email == accUsername) && a.AccPassword == accPassword && a.IsActive == true);
            return account;
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == accId);
                if (account == null)
                {
                    return false;
                }

                // Delete all patients associated with this account first
                var relatedPatients = await _context.Patients
                    .Where(p => p.AccId == accId)
                    .ToListAsync();

                if (relatedPatients.Any())
                {
                    _context.Patients.RemoveRange(relatedPatients);
                }

                // Now delete the account
                _context.Accounts.Remove(account);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        public async Task<Patient> CreatePatientAccountAsync(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            // Ensure the Account object is present
            if (patient.Acc == null)
            {
                throw new ArgumentException("Patient must have an associated Account object.", nameof(patient));
            }

            // Add Account first
            _context.Accounts.Add(patient.Acc);
            await _context.SaveChangesAsync();

            // Set the foreign key
            patient.AccId = patient.Acc.AccId;

            // Add Patient
            _context.Set<Patient>().Add(patient);
            await _context.SaveChangesAsync();

            return patient;
        }

        public Task<bool> IsEmailUsedAsync(string mail)
        {
            return _context.Accounts.AnyAsync(a => a.Email == mail);
        }

        public async Task<Account> UpdateAccountProfileAsync(int id, Account updatedAccount)
        {
            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with id {id} not found.");
            }

            // Update only profile fields
            existingAccount.AccPassword = updatedAccount.AccPassword ?? existingAccount.AccPassword;
            existingAccount.Email = updatedAccount.Email ?? existingAccount.Email;
            existingAccount.Fullname = updatedAccount.Fullname ?? existingAccount.Fullname;
            existingAccount.Dob = updatedAccount.Dob ?? existingAccount.Dob;
            existingAccount.Gender = updatedAccount.Gender ?? existingAccount.Gender;

            await _context.SaveChangesAsync();
            return existingAccount;
        }

        public async Task<Account?> GetAccountByUsernameAsync(string username)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccUsername == username);
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == email.ToLower());
        }

        public async Task<Account> UpdateAccountStatusAsync(int id, bool isActive)
        {
            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with id {id} not found.");
            }

            // Update only the IsActive status
            existingAccount.IsActive = isActive;

            await _context.SaveChangesAsync();
            return existingAccount;
        }

        public async Task<bool> ChangePasswordAsync(int accId, ChangePasswordRequestDTO request)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.AccId == accId);
            if (account == null)
            {
                return false;
            }

            account.AccPassword = request.newPassword!;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
