using FirstIterationProductRelease.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FirstIterationProductRelease.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IActivityLogRepository _activityLogRepository;

        public static Account ActiveAccount;

        public AccountController(IAccountRepository accountRepository, IActivityLogRepository activityLogRepository)
        {
            _accountRepository = accountRepository;
            _activityLogRepository = activityLogRepository;
        }

        public Account GetAccountById(int id)
        {
            foreach (Account acc in _accountRepository.Accounts)
            {
                if (acc.Id == id)
                {
                    return acc;
                }
            }
            return null;
        }

        public Account GetAccountByUsername(string username)
        {
            foreach (Account acc in _accountRepository.Accounts)
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
            var accountDetail = new Account
            {
                LegalName = "Jacob Yoast",
                Email = "jacob.yoast@gmail.com",
                ScholarshipStatus = "Pending",
            };

            return View(accountDetail);
        }

        public ViewResult AccountList()
        {
            return View(_accountRepository.Accounts);
        }

        public ViewResult AccountCreation()
        {
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
                _accountRepository.SaveAccount(acc);
                return View("AccountList", _accountRepository.Accounts);
            }
            else
            {
                return View("AccountSettings", acc);
            }
        }

        public ViewResult SignIn(Account attempt)
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

        public ViewResult LogInPage()
        {
            return View();
        }

        [HttpPost]
        public ViewResult NewAccount(Account acc)
        {
            if (!ModelState.IsValid)
                return View("AccountCreation");

            _accountRepository.SaveAccount(acc);
            return View("AccountList", _accountRepository.Accounts);
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

    public class ActivityLog
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
