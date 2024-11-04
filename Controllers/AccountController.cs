using FirstIterationProductRelease.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstIterationProductRelease.Controllers
{
    public class AccountController : Controller
    {
        private IAccountRepository AccountRepository;


        public static Account ActiveAccount;
        public AccountController(IAccountRepository AccountRepository)
        {
            this.AccountRepository = AccountRepository;
        }
        
        public Account GetAccountById(int id) {
            foreach (Account acc in AccountRepository.Accounts) {
                if (acc.Id == id)
                {
                    return acc;
                }
            }
            return null;
        }

        public Account GetAccountByUsername(string username)
        {
            foreach (Account acc in AccountRepository.Accounts)
            {
                if (acc.Username == username)
                {
                    return acc;
                }
            }
            return null;
        }

        public ActionResult AccountDetails()
        {
            // simulates data fetching from database
            var accountDetail = new Account
            {
                LegalName = "Jacob Yoast",
                Email = "jacob.yoast@gmail.com",
                ScholarshipStatus = "Pending",
            };

            return View(accountDetail);
        }


        public ViewResult AccountList() {
            return View(AccountRepository.Accounts);
        }

        public ViewResult AccountCreation() {
            return View();
        }

        public ViewResult AccountSettings(int id)
        {
            return View(GetAccountById(id));
        }


        [HttpPost]
        public IActionResult Edit(Account acc)
        {
            if (ModelState.IsValid)
            {
                AccountRepository.SaveAccount(acc);
                return View("AccountList", AccountRepository.Accounts);
            }
            else
            {
                // there is something wrong with the data values
                return View("AccountSettings",acc);
            }
        }

        public ViewResult SignIn(Account attempt) {
            Console.WriteLine(attempt.Username + ", " + attempt.Password);
            Account acc = GetAccountByUsername(attempt.Username);
            if (acc == null || acc.Password != attempt.Password) return View("LogInPage");
            else ActiveAccount = acc;
            Console.WriteLine(ActiveAccount.Username + " is the active account");
            return View("Index");
        }

        public ViewResult LogInPage() {
            return View();
        }

        [HttpPost]
        public ViewResult NewAccount(Account acc)
        {
            if (!ModelState.IsValid) return View("AccountCreation");
            Random rand = new Random(System.DateTime.Now.Millisecond);
            AccountRepository.SaveAccount(acc);
            return View("AccountList", AccountRepository.Accounts);
        }


        // log code abdel-

        private IAccountRepository _accountRepository;
        private IActivityLogRepository _activityLogRepository;

        public AccountController(IAccountRepository accountRepository, IActivityLogRepository activityLogRepository)
        {
            _accountRepository = accountRepository;
            _activityLogRepository = activityLogRepository;
        }

        public ViewResult SignInAttempt(Account attempt)
        {
            Console.WriteLine(attempt.Username + ", " + attempt.Password);
            Account acc = GetAccountByUsername(attempt.Username);
            if (acc == null || acc.Password != attempt.Password)
                return View("LogInPage");
            else
                ActiveAccount = acc;

            Console.WriteLine(ActiveAccount.Username + " is the active account");
            return View("Index");
        }

        public ViewResult ActivityLog()
        {
            var logs = _activityLogRepository.GetLogsForAccount(ActiveAccount.Id);
            return View(logs);
        }


    }

    // attenpting to create activity log -abdel
    public class ActivityLog
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
