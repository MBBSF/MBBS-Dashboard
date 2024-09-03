
namespace VolunteerManagementSystem.Models
{
    public interface IAccountRepository
    {
        IEnumerable<Account> Accounts { get; }
        public void SaveAccount(Account account);
    }
}