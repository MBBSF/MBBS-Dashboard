using FirstIterationProductRelease.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace FirstIterationProductRelease.Models
{
    public class EFActivityLogRepository : IActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EFActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddLog(ActivityLog log)
        {
            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }

        public IEnumerable<ActivityLog> GetLogsForAccount(int accountId)
        {
            return _context.ActivityLogs
                           .Where(log => log.AccountId == accountId)
                           .OrderByDescending(log => log.Timestamp);
        }
    }
}
