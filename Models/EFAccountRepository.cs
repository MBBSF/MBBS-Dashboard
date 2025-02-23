using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
                _context.Accounts.Add(account);
            }
            else
            {
                var dbEntry = _context.Accounts.FirstOrDefault(a => a.Id == account.Id);
                if (dbEntry != null)
                {
                    dbEntry.LegalName = account.LegalName;
                    dbEntry.Email = account.Email;
                    dbEntry.Password = HashPassword(account.Password);
                }
            }
            _context.SaveChanges();
        }

        public Account AuthenticateUser(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            return _context.Accounts.FirstOrDefault(a => a.Username == username && a.Password == hashedPassword);
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}