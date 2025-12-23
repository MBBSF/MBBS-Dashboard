using System.Linq;

namespace MBBS.Dashboard.web.Models
{
    public class EFAccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;
        public IEnumerable<Account> Accounts => _context.Accounts;

        public EFAccountRepository(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public void SaveAccount(Account account)
        {
            if (account.Id == 0)
            {
                // For new accounts, add the account directly.
                _context.Accounts.Add(account);
            }
            else
            {
                // For existing accounts, update the properties.
                var dbEntry = _context.Accounts.FirstOrDefault(a => a.Id == account.Id);
                if (dbEntry != null)
                {
                    dbEntry.LegalName = account.LegalName;
                    dbEntry.Email = account.Email;
                    // Update the hashed password.
                    dbEntry.Password = account.Password;
                }
            }
            _context.SaveChanges();
        }

        public Account AuthenticateUser(string username, string password)
        {
            // NOTE: This method is deprecated - use password hashing in the controller instead
            // Compare the plain-text password directly (legacy method).
            return _context.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);
        }
    }
}