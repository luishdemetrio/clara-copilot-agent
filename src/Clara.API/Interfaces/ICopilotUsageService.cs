namespace Clara.API.Interfaces;


using Clara.API.Models;

public interface ICopilotUsageService
{
    Task<IEnumerable<M365CopilotUsageReport>> GetInactiveUsersAsync();
}
