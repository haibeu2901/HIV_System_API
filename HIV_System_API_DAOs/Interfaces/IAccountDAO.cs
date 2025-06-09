using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IAccountDAO
    {
        Task<List<Account>> GetAllAccountsAsync();
        Task<Account> GetAccountByLoginAsync(string accUsername, string accPassword);
    }
}
