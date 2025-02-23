using System.Collections.Generic;

namespace MBBS.Dashboard.web.Models
{
    public interface IAccountRepository
    {
        IEnumerable<Account> Accounts { get; }
        void SaveAccount(Account account);
        Account AuthenticateUser(string username, string password); // abdel fix
        string HashPassword(string password); // abdel fix
    }
}