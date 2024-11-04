using FirstIterationProductRelease.Controllers;
using System.Collections.Generic;

namespace FirstIterationProductRelease.Models
{
    public interface IActivityLogRepository
    {
        void AddLog(ActivityLog log);
        IEnumerable<ActivityLog> GetLogsForAccount(int accountId);
    }
}