using MBBS.Dashboard.web.Controllers;
using System.Collections.Generic;

namespace MBBS.Dashboard.web.Models
{
    public interface IActivityLogRepository
    {
        void AddLog(ActivityLog log);
        IEnumerable<ActivityLogViewModel> GetLogsForAccount(int accountId);
        Task<List<ActivityLogViewModel>> GetRecentActivityLogsAsync(int count, int? accountId = null);
    }
}