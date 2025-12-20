using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBBS.Dashboard.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IPasswordHasher<Account> _passwordHasher;

        // For demo purposes only.
        // In a production app, use a proper authentication mechanism.
        public static Account ActiveAccount;

        public AccountController(IAccountRepository accountRepository, IActivityLogRepository activityLogRepository, IPasswordHasher<Account> passwordHasher)
        {
            _accountRepository = accountRepository;
            _activityLogRepository = activityLogRepository;
            _passwordHasher = passwordHasher;
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
                // Verify current password using password hasher
                var verificationResult = _passwordHasher.VerifyHashedPassword(ActiveAccount, ActiveAccount.Password, model.CurrentPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }
                
                // Hash the new password
                ActiveAccount.Password = _passwordHasher.HashPassword(ActiveAccount, model.NewPassword);
                _accountRepository.SaveAccount(ActiveAccount);
                
                // Log the password change action
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = ActiveAccount.Id,
                    Action = "Password Changed",
                    Timestamp = DateTime.UtcNow,
                    Details = $"User {ActiveAccount.Username} changed their password"
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
                // Verify the provided current password using password hasher
                var verificationResult = _passwordHasher.VerifyHashedPassword(ActiveAccount, ActiveAccount.Password, model.CurrentPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
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
                    Timestamp = DateTime.UtcNow,
                    Details = $"User {ActiveAccount.Username} updated their account details"
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

            // Check if username already exists
            if (_accountRepository.Accounts.Any(a => a.Username.ToLower() == acc.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View("AccountCreation");
            }

            // Check if email already exists
            if (_accountRepository.Accounts.Any(a => a.Email.ToLower() == acc.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View("AccountCreation");
            }

            // Allow the admin to set the role. If not provided, set a default.
            if (string.IsNullOrWhiteSpace(acc.UserRole))
            {
                acc.UserRole = "User";
            }

            // Default new accounts are active.
            acc.IsActive = true;

            // Hash the password before saving
            acc.Password = _passwordHasher.HashPassword(acc, acc.Password);

            _accountRepository.SaveAccount(acc);
            // Log the account creation action
            _activityLogRepository.AddLog(new ActivityLog
            {
                AccountId = acc.Id,
                Action = "Account Created",
                Timestamp = DateTime.UtcNow,
                Details = $"Admin created new account for user {acc.Username}"
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
            
            // Map to view model to avoid password validation issues
            var viewModel = new AdminEditAccountViewModel
            {
                Id = account.Id,
                LegalName = account.LegalName,
                Username = account.Username,
                Email = account.Email,
                UserRole = account.UserRole,
                IsActive = account.IsActive
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(AdminEditAccountViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            if (ModelState.IsValid)
            {
                // Get the existing account from database
                var existingAccount = GetAccountById(model.Id);
                if (existingAccount == null)
                {
                    return View("Error");
                }

                // Safety check: prevent removing all admin accounts
                if (existingAccount.UserRole == "Admin" && model.UserRole != "Admin")
                {
                    var adminCount = _accountRepository.Accounts.Count(a => a.UserRole == "Admin" && a.IsActive);
                    if (adminCount <= 1)
                    {
                        ModelState.AddModelError("UserRole", "Cannot change role: At least one active admin account must remain.");
                        return View("AccountSettings", model);
                    }
                }

                // Check if username is already taken by another user
                if (existingAccount.Username != model.Username)
                {
                    var usernameExists = _accountRepository.Accounts.Any(a => a.Username.ToLower() == model.Username.ToLower() && a.Id != model.Id);
                    if (usernameExists)
                    {
                        ModelState.AddModelError("Username", "Username is already taken by another user.");
                        return View("AccountSettings", model);
                    }
                }

                // Check if email is already taken by another user
                if (existingAccount.Email != model.Email)
                {
                    var emailExists = _accountRepository.Accounts.Any(a => a.Email.ToLower() == model.Email.ToLower() && a.Id != model.Id);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Email is already registered to another user.");
                        return View("AccountSettings", model);
                    }
                }

                // Update account properties
                existingAccount.LegalName = model.LegalName;
                existingAccount.Username = model.Username;
                existingAccount.Email = model.Email;
                existingAccount.UserRole = model.UserRole;
                existingAccount.IsActive = model.IsActive;

                // Only update password if a new one is provided
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    existingAccount.Password = _passwordHasher.HashPassword(existingAccount, model.NewPassword);
                }

                _accountRepository.SaveAccount(existingAccount);
                
                // Build detailed log message
                var changes = new List<string>();
                if (!string.IsNullOrWhiteSpace(model.NewPassword)) 
                    changes.Add("password updated");
                if (existingAccount.UserRole != model.UserRole) 
                    changes.Add($"role changed to {model.UserRole}");
                if (existingAccount.IsActive != model.IsActive) 
                    changes.Add($"status set to {(model.IsActive ? "active" : "inactive")}");
                
                string changeDetails = changes.Any() ? $" ({string.Join(", ", changes)})" : "";
                
                // Log the admin account edit action
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = model.Id,
                    Action = "Account Edited by Admin",
                    Timestamp = DateTime.UtcNow,
                    Details = $"Admin edited account for user {model.Username}{changeDetails}"
                });
                return RedirectToAction("AccountList");
            }
            return View("AccountSettings", model);
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
            string actionMessage = isActive ? "Account Approved & Activated" : "Account Deactivated";
            string detailMessage = isActive 
                ? $"Admin approved and activated account for user {account.Username}"
                : $"Admin deactivated account for user {account.Username}";
                
            _activityLogRepository.AddLog(new ActivityLog
            {
                AccountId = account.Id,
                Action = actionMessage,
                Timestamp = DateTime.UtcNow,
                Details = detailMessage
            });

            return RedirectToAction("AccountList");
        }

        // --------------------------
        // Authentication Actions:
        // --------------------------
        public IActionResult SignIn(Account attempt)
        {
            // Find user by username first
            var existingAccount = _accountRepository.Accounts.FirstOrDefault(a => a.Username == attempt.Username);
            
            if (existingAccount == null)
            {
                ViewBag.ErrorMessage = "Invalid login credentials.";
                return View("LogInPage");
            }

            // Verify password using password hasher
            var verificationResult = _passwordHasher.VerifyHashedPassword(existingAccount, existingAccount.Password, attempt.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ViewBag.ErrorMessage = "Invalid login credentials.";
                return View("LogInPage");
            }

            if (!existingAccount.IsActive)
            {
                ViewBag.ErrorMessage = "Your account is pending admin approval. Please contact an administrator to activate your account before you can log in.";
                return View("LogInPage");
            }
            
            ActiveAccount = existingAccount;
            _activityLogRepository.AddLog(new ActivityLog
            {
                AccountId = existingAccount.Id,
                Action = "Signed In",
                Timestamp = DateTime.UtcNow,
                Details = $"User {existingAccount.Username} signed in successfully"
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
                    Timestamp = DateTime.UtcNow,
                    Details = $"User {ActiveAccount.Username} logged out"
                });
            }
            ActiveAccount = null;
            return RedirectToAction("LogInPage");
        }

        public IActionResult LogInPage()
        {
            return View();
        }

        // Public user registration - accessible to all
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Account account, string confirmPassword)
        {
            // Set default values BEFORE validation
            account.UserRole = "User"; // Default role for self-registration  
            account.IsActive = false;  // New users require admin activation
            
            // Clear any UserRole validation errors since we're setting it
            if (ModelState.ContainsKey("UserRole"))
            {
                ModelState.Remove("UserRole");
            }
            
            if (!ModelState.IsValid)
            {
                return View(account);
            }

            // Check password confirmation
            if (account.Password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(account);
            }

            // Check if username already exists
            if (_accountRepository.Accounts.Any(a => a.Username.ToLower() == account.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(account);
            }

            // Check if email already exists
            if (_accountRepository.Accounts.Any(a => a.Email.ToLower() == account.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(account);
            }

            // Additional password validation
            if (account.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
                return View(account);
            }

            try
            {
                // Hash the password before saving
                account.Password = _passwordHasher.HashPassword(account, account.Password);
                
                _accountRepository.SaveAccount(account);
                
                // Log the registration
                _activityLogRepository.AddLog(new ActivityLog
                {
                    AccountId = account.Id,
                    Action = "User Registered (Pending Approval)",
                    Timestamp = DateTime.UtcNow,
                    Details = $"New user {account.Username} registered and is awaiting admin approval"
                });

                TempData["SuccessMessage"] = "Registration successful! Your account has been created but requires admin approval before you can log in. Please contact an administrator to activate your account.";
                return RedirectToAction("LogInPage");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(account);
            }
        }
    }

    public class ActivityLog
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } // Add this property
    }
}