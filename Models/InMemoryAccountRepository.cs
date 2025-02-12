using System.Collections.Generic;
using System.Linq;

namespace MBBS.Dashboard.web.Models
{
    /// abdel memory database
    public class InMemoryAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts = new List<Account>
        {
            // Pre-populated account for demonstration purposes.
            new Account
            {
                Id = 1,
                LegalName = "Jacob Yoast",
                Username = "jacoby",
                Password = "password123",
                Email = "jacob.yoast@gmail.com",
                UserRole = "Admin"
            }
        };


        /// Provides access to the list of accounts.
        public IEnumerable<Account> Accounts => _accounts;

        public void SaveAccount(Account account)
        {
            var existingAccount = _accounts.FirstOrDefault(a => a.Id == account.Id);
            if (existingAccount != null)
            {
                // Update existing account details
                existingAccount.LegalName = account.LegalName;
                existingAccount.Email = account.Email;
                existingAccount.UserRole = account.UserRole;
                existingAccount.Password = account.Password;
            }
            else
            {
                // Add new account if it doesn't already exist
                _accounts.Add(account);
            }
        }
    }
}
