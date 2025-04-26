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
                // Check if the current password entered matches the active account's password.
                if (ActiveAccount.Password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }
                ActiveAccount.Password = model.NewPassword; // Assuming NewPassword is provided.
                _accountRepository.SaveAccount(ActiveAccount);
                // Log the password change action
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = ActiveAccount.Id,
                    Action = "Password Changed",
                    Timestamp = DateTime.UtcNow
                });
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
            // Populate the view model with the current values.
            var model = new EditAccountViewModel
            {
                Id = ActiveAccount.Id,
                LegalName = ActiveAccount.LegalName,
                Email = ActiveAccount.Email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAccount(EditAccountViewModel model)
        {
            if (ActiveAccount == null)
            {
                return RedirectToAction("LogInPage");
            }

            if (ModelState.IsValid)
            {
                // Verify the provided current password.
                if (ActiveAccount.Password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }
                // Update the account's legal name and email.
                ActiveAccount.LegalName = model.LegalName;
                ActiveAccount.Email = model.Email;

                _accountRepository.SaveAccount(ActiveAccount);
                // Log the account edit action
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = ActiveAccount.Id,
                    Action = "Account Details Updated",
                    Timestamp = DateTime.UtcNow
                });
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToAction("AccountDetails");
            }
            return View(model);
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
            // Log the account creation action
            _activityLogRepository.AddLog(new ActivityLog
            {
                AccountId = acc.Id,
                Action = "Account Created",
                Timestamp = DateTime.UtcNow
            });

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
                // Log the admin account edit action
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = acc.Id,
                    Action = "Account Edited by Admin",
                    Timestamp = DateTime.UtcNow
                });
                return RedirectToAction("AccountList");
            }
            return View("AccountSettings", acc);
        }

        // New ADMIN-only action: Display account details for any account.
        [HttpGet]
        public IActionResult Details(int id)
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
            return View(account);
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
            // Log the account status change action
            _activityLogRepository.AddLog(new ActivityLog
            {
                AccountId = account.Id,
                Action = isActive ? "Account Activated" : "Account Deactivated",
                Timestamp = DateTime.UtcNow
            });

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
            // Log the sign-in action
            _activityLogRepository.AddLog(new ActivityLog
            {
                AccountId = acc.Id,
                Action = "Signed In",
                Timestamp = DateTime.UtcNow
            });
            return RedirectToAction("Index", "Home");
        }

        public IActionResult LogOut()
        {
            // Log the logout action before clearing ActiveAccount
            if (ActiveAccount != null)
            {
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = ActiveAccount.Id,
                    Action = "Logged Out",
                    Timestamp = DateTime.UtcNow
                });
            }
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