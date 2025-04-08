using MBBS.Dashboard.web.Controllers;
using System.Collections.Generic;

namespace MBBS.Dashboard.web.Models
{
    public interface IActivityLogRepository
    {
        void AddLog(ActivityLog log);
        IEnumerable<ActivityLog> GetLogsForAccount(int accountId);
        Task<IEnumerable<object>> GetRecentActivityLogsAsync(int v);
    }
}