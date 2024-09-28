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


    }
}
