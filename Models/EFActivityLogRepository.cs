using MBBS.Dashboard.web.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBBS.Dashboard.web.Models
{
    public class EFActivityLogRepository : IActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EFActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to extract username from Details
        private string ExtractUserNameFromDetails(string details)
        {
            if (string.IsNullOrEmpty(details))
            {
                Console.WriteLine("Details is null or empty");
                return "Unknown User";
            }

            // Convert to lowercase for case-insensitive matching
            string lowerDetails = details.ToLowerInvariant();
            Console.WriteLine($"Lowered Details: '{lowerDetails}'");

            // Define patterns
            const string userPrefix1 = "user "; // For "User {username} ..."
            const string userPrefix2 = "for user "; // For "Admin ... for user {username} ..."
            const string userPrefix3 = "user "; // For "Admin set account status for user {username} to ..."

            if (lowerDetails.StartsWith(userPrefix1))
            {
                Console.WriteLine("Matched 'user ' prefix");
                int startIndex = userPrefix1.Length;
                // Look for the next keyword that indicates the end of the username
                string[] endKeywords = { " signed in", " logged out", " changed their", " updated their" }; // Adjusted keywords
                int endIndex = details.Length;
                Console.WriteLine($"StartIndex: {startIndex}, Initial EndIndex: {endIndex}");
                foreach (var keyword in endKeywords)
                {
                    int keywordIndex = lowerDetails.IndexOf(keyword, startIndex);
                    Console.WriteLine($"Looking for keyword '{keyword}' at position {keywordIndex}");
                    if (keywordIndex != -1 && keywordIndex < endIndex)
                    {
                        endIndex = keywordIndex;
                        Console.WriteLine($"Updated EndIndex to {endIndex} for keyword '{keyword}'");
                    }
                }
                string extractedUserName = details.Substring(startIndex, endIndex - startIndex);
                Console.WriteLine($"Extracted UserName: '{extractedUserName}'");
                return extractedUserName;
            }
            else if (lowerDetails.Contains(userPrefix2))
            {
                Console.WriteLine("Matched 'for user ' prefix");
                int startIndex = details.ToLower().IndexOf(userPrefix2) + userPrefix2.Length;
                int endIndex = details.Length;
                string extractedUserName = details.Substring(startIndex, endIndex - startIndex);
                Console.WriteLine($"Extracted UserName: '{extractedUserName}'");
                return extractedUserName;
            }
            else if (lowerDetails.Contains(userPrefix3))
            {
                Console.WriteLine("Matched 'user ' prefix (for status)");
                int startIndex = details.ToLower().IndexOf(userPrefix3) + userPrefix3.Length;
                int endIndex = lowerDetails.IndexOf(" to ", startIndex);
                if (endIndex == -1) endIndex = details.Length;
                string extractedUserName = details.Substring(startIndex, endIndex - startIndex);
                Console.WriteLine($"Extracted UserName: '{extractedUserName}'");
                return extractedUserName;
            }

            Console.WriteLine("No matching pattern found");
            return "Unknown User";
        }

        public async Task<List<ActivityLogViewModel>> GetRecentActivityLogsAsync(int count, int? accountId = null)
        {
            var query = _context.ActivityLogs.AsQueryable();

            if (accountId.HasValue)
            {
                query = query.Where(log => log.AccountId == accountId.Value);
            }

            var logs = await query
                .Select(log => new ActivityLogViewModel
                {
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    UserName = "Temp", // Will be overwritten
                    Details = log.Details ?? "No details available"
                })
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToListAsync();

            // Debugging: Log the Details field values
            Console.WriteLine("Activity Log Details:");
            foreach (var log in logs)
            {
                Console.WriteLine($"Details: '{log.Details}'");
            }

            foreach (var log in logs)
            {
                log.UserName = ExtractUserNameFromDetails(log.Details);
            }

            return logs;
        }

        public IEnumerable<ActivityLogViewModel> GetLogsForAccount(int accountId)
        {
            var logs = _context.ActivityLogs
                .Where(x => x.AccountId == accountId)
                .Select(log => new ActivityLogViewModel
                {
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    UserName = "Temp", // Will be overwritten
                    Details = log.Details ?? "No details available"
                })
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            foreach (var log in logs)
            {
                log.UserName = ExtractUserNameFromDetails(log.Details);
            }

            return logs;
        }

        public void AddLog(ActivityLog log)
        {
            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}