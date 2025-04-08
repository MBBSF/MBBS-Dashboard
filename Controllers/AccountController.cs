using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MBBS.Dashboard.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IActivityLogRepository _activityLogRepository;

        // For demo purposes only.
        // In a production app, use a proper authentication mechanism.
        public static Account ActiveAccount;

        public AccountController(IAccountRepository accountRepository, IActivityLogRepository activityLogRepository)
        {
            _accountRepository = accountRepository;
            _activityLogRepository = activityLogRepository;
        }

        // Helper method to check if the active user is an Admin.
        private bool IsAdmin()
        {
            return ActiveAccount != null &&
                   ActiveAccount.UserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        // Action to display Access Denied.
        public IActionResult AccessDenied()
        {
            return View(); // Create Views/Account/AccessDenied.cshtml
        }

        // Utility methods.
        public Account GetAccountById(int id)
        {
            return _accountRepository.Accounts.FirstOrDefault(acc => acc.Id == id);
        }

        public Account GetAccountByUsername(string username)
        {
            return _accountRepository.Accounts.FirstOrDefault(acc => acc.Username == username);
        }

        // --------------------------
        // Actions accessible to all logged-in users:
        // --------------------------
        public IActionResult AccountDetails()
        {
            if (ActiveAccount == null)
            {
                return RedirectToAction("LogInPage");
            }
            return View(ActiveAccount);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (ModelState.IsValid)
            {
                // Implement your plain-text password change logic here.
                // For example:
                ActiveAccount.Password = model.NewPassword; // Assuming NewPassword is provided.
                _accountRepository.SaveAccount(ActiveAccount);
                return View("PasswordChangeSuccessful");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult EditAccount()
        {
            if (ActiveAccount == null)
            {
                return RedirectToAction("LogInPage");
            }
            return View(ActiveAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAccount(Account updatedAccount)
        {
            if (ActiveAccount == null)
            {
                return RedirectToAction("LogInPage");
            }

            if (ModelState.IsValid)
            {
                // Update only allowed fields.
                ActiveAccount.LegalName = updatedAccount.LegalName;
                ActiveAccount.Email = updatedAccount.Email;

                if (!string.IsNullOrEmpty(updatedAccount.Password))
                {
                    // Store the plain-text password directly.
                    ActiveAccount.Password = updatedAccount.Password;
                }

                _accountRepository.SaveAccount(ActiveAccount);
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToAction("AccountDetails");
            }
            return View(updatedAccount);
        }

        public IActionResult ActivityLog()
        {
            if (ActiveAccount == null)
            {
                return RedirectToAction("LogInPage");
            }
            var logs = _activityLogRepository.GetLogsForAccount(ActiveAccount.Id);
            return View(logs);
        }

        // --------------------------
        // ADMIN-ONLY actions:
        // --------------------------
        public IActionResult AccountList()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }
            return View(_accountRepository.Accounts);
        }

        public IActionResult AccountCreation()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }
            return View();
        }

        [HttpPost]
        public IActionResult NewAccount(Account acc)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                return View("AccountCreation");
            }

            // Allow the admin to set the role. If not provided, set a default.
            if (string.IsNullOrWhiteSpace(acc.UserRole))
            {
                acc.UserRole = "User";
            }

            // Default new accounts are active.
            acc.IsActive = true;

            // Save the plain-text password directly.
            _accountRepository.SaveAccount(acc);

            return RedirectToAction("AccountList");
        }

        public IActionResult AccountSettings(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            var account = GetAccountById(id);
            if (account == null)
            {
                return View("Error"); // Or a NotFound view.
            }
            return View(account);
        }

        [HttpPost]
        public IActionResult Edit(Account acc)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            if (ModelState.IsValid)
            {
                _accountRepository.SaveAccount(acc);
                return RedirectToAction("AccountList");
            }
            return View("AccountSettings", acc);
        }

        // New ADMIN-ONLY action to toggle account active status.
        [HttpPost]
        public IActionResult SetAccountStatus(int id, bool isActive)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            var account = GetAccountById(id);
            if (account == null)
            {
                return View("Error");
            }

            account.IsActive = isActive;
            _accountRepository.SaveAccount(account);

            return RedirectToAction("AccountList");
        }

        // --------------------------
        // Authentication Actions:
        // --------------------------
        public IActionResult SignIn(Account attempt)
        {
            // Authenticate using plain-text comparisons.
            Account acc = _accountRepository.AuthenticateUser(attempt.Username, attempt.Password);

            // Check for valid credentials and that the account is active.
            if (acc == null)
            {
                ViewBag.ErrorMessage = "Invalid login credentials.";
                return View("LogInPage");
            }
            if (!acc.IsActive)
            {
                ViewBag.ErrorMessage = "Your account is inactive. Please contact an administrator.";
                return View("LogInPage");
            }
            ActiveAccount = acc;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult LogOut()
        {
            ActiveAccount = null;
            return RedirectToAction("LogInPage");
        }

        public IActionResult LogInPage()
        {
            return View();
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