namespace Clara.API.Interfaces;


using Clara.API.Models;

public interface ICopilotUsageService
{
    Task<IEnumerable<M365CopilotUsageReport>> GetInactiveUsersAsync(int? days = null, int? topUsers = null);
}
