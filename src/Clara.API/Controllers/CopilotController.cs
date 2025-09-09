using Clara.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clara.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CopilotController : ControllerBase
{
    
    private readonly ICopilotLicenseService _licenseService;
    private readonly ICopilotUsageService _usageService;
    private readonly ICopilotGroupService _groupService;

    
    public CopilotController(ICopilotLicenseService licenseService,
                             ICopilotUsageService usageService,
                             ICopilotGroupService groupService)
    {
        
        _licenseService = licenseService;
        _usageService = usageService;
        _groupService = groupService;
    }

    [Authorize]
    [HttpGet("license-counts")]
    public async Task<IActionResult> GetCopilotLicenseAvailability()
    {        
        var counts = await _licenseService.GetLicenseCountsAsync();
        return Ok(counts);       
    }


    /// <summary>
    /// Retrieves the full usage report from the M365 Copilot Usage Report.
    /// This includes user activity across Copilot-enabled apps and is used to identify inactive or low-usage users.
    /// </summary>
    /// <param name="days">
    /// The number of days to look back from today to identify users who have not accessed Copilot.
    /// For example, days=30 will return users inactive for the past 30 days.
    /// </param>
    /// <param name="topUsers">
    /// The number of users to return in the report, based on the filter criteria (e.g., least active).
    /// For example, topUsers=10 will return the top 10 inactive users.
    /// </param>
    /// <returns>
    /// A list of users matching the specified inactivity and filter criteria.
    /// </returns>

    [HttpGet("usage-report")]
    [Authorize]
    public async Task<IActionResult> GetM365CopilotUsageReport(int? days = null, int? topUsers = null)
    {
        var inactive = await _usageService.GetInactiveUsersAsync(days, topUsers);
        return Ok(inactive);
    }

    [Authorize]

    
    // Remove Copilot license from user
    [Authorize]
    [HttpPost("remove-license/{userId}")]
    public async Task<IActionResult> RemoveCopilotLicense(string userId)
    {
        var result = await _licenseService.RemoveLicenseByIdAsync(userId);
        if (!result)
            return NotFound(new { message = $"User with id {userId} not found." });

        return Ok(new { userId, removed = true });
    }


    [Authorize]
    [HttpPost("remove-license-by-email/{userEmail}")]
    public async Task<IActionResult> RemoveCopilotLicenseByEmail(string userEmail)
    {
        var result = await _licenseService.RemoveLicenseByEmailAsync(userEmail);
        if (!result)
            return NotFound(new { message = $"User with email {userEmail} not found." });

        return Ok(new { userEmail, removed = true });        
    }

    [Authorize]
    [HttpPost("assign-license/{userId}")]
    public async Task<IActionResult> AssignCopilotLicense(string userId)
    {
        var result = await _licenseService.AssignLicenseByIdAsync(userId);
        if (!result)
            return NotFound(new { message = $"User with id {userId} not found." });

        return Ok(new { userId, assigned = true });

    }

    [Authorize]
    [HttpPost("assign-license-by-email/{userEmail}")]
    public async Task<IActionResult> AssignCopilotLicenseByEmail(string userEmail)
    {
        
        var result = await _licenseService.AssignLicenseByEmailAsync(userEmail);

        if (!result)
            return NotFound(new { message = $"User with email {userEmail} not found." });

        return Ok(new { userEmail, assigned = true });

    }


    [Authorize]
    [HttpPost("add-user-to-group/{userEmail}")]
    public async Task<IActionResult> AddUserToGroup(string userEmail)
    {   
        var result = await _groupService.AddUserToGroupAsync(userEmail);

        if (!result)
            return NotFound(new { message = $"User or group not found." });

        return Ok(new { userEmail, added = true });
       
    }

    [Authorize]
    [HttpPost("remove-user-from-group/{userEmail}")]
    public async Task<IActionResult> RemoveUserFromGroup(string userEmail)
    {
    
        var result = await _groupService.RemoveUserFromGroupAsync(userEmail);
        if (!result)
            return NotFound(new { message = $"User or group not found." });

        return Ok(new { userEmail, removed = true });

    }


}
