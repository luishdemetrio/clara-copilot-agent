namespace Clara.API.Interfaces;

using Clara.API.Models;

public interface ICopilotLicenseService
{
    Task<bool> AssignLicenseByEmailAsync(string userEmail);
    Task<bool> AssignLicenseByIdAsync(string userId);
    Task<bool> RemoveLicenseByEmailAsync(string userEmail);
    Task<bool> RemoveLicenseByIdAsync(string userId);
    Task<LicenseCountsDto> GetLicenseCountsAsync();
}



