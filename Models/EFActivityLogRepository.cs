using MBBS.Dashboard.web.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MBBS.Dashboard.web.Models
{
    public class EFActivityLogRepository : IActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EFActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count)
        {
            return await _context.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public void AddLog(ActivityLog log)
        {
            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }

        public IEnumerable<ActivityLog> GetLogsForAccount(int accountId)
        {
            return _context.ActivityLogs
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }


}
