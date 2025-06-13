
using Azure.Identity;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Globalization;
using System.Net.Http.Headers;

namespace Clara.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CopilotController : ControllerBase
{
    private readonly GraphServiceClient _graphClient;
    private readonly ClientSecretCredential _credential;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _copilotSkuId;
    private readonly IConfiguration _config;

    public CopilotController(
        GraphServiceClient graphClient,
        ClientSecretCredential credential,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _graphClient = graphClient;
        _credential = credential;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _copilotSkuId = _config["CopilotSkuId"]!;
    }

    // 1. List users with Copilot license

    
    [HttpGet("usage-report")]
    [Authorize]
    public async Task<IActionResult> GetM365CopilotUsageReport()
    {
        var users = await _graphClient.Users
            .GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Filter = $"assignedLicenses/any(x:x/skuId eq {_copilotSkuId})";
                requestConfig.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName" };
            });

        var usageList = await GetCopilotUsageReport();

        // Build a dictionary for quick lookup by UPN
        var usageDict = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        foreach (dynamic record in usageList)
        {
            var upn = record?.userPrincipalName as string;
            if (!string.IsNullOrEmpty(upn))
                usageDict[upn] = record!;
        }

        var inactive = users!.Value!
            .Where(u => !usageDict.ContainsKey(u.UserPrincipalName!))
            .Select(u =>
            {
                // Try to get usage record (should be null for inactive, but you can still include fields)
                dynamic? usageRecord = null;
                usageDict.TryGetValue(u.UserPrincipalName!, out usageRecord);

                return new M365CopilotUsageReport
                {
                    UserId = u.Id!,
                    UserDisplayName= u.DisplayName!,
                    UserPrincipalName = u.UserPrincipalName!,
                    LastActivityDate = ParseNullableDateTime(usageRecord?.lastActivityDate),
                    CopilotChatLastActivityDate = ParseNullableDateTime(usageRecord?.copilotChatLastActivityDate),
                    MicrosoftTeamsCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.microsoftTeamsCopilotLastActivityDate),
                    WordCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.wordCopilotLastActivityDate),
                    ExcelCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.excelCopilotLastActivityDate),
                    PowerPointCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.powerPointCopilotLastActivityDate),
                    OutlookCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.outlookCopilotLastActivityDate),
                    OneNoteCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.oneNoteCopilotLastActivityDate),
                    LoopCopilotLastActivityDate = ParseNullableDateTime(usageRecord?.loopCopilotLastActivityDate)
                };
            });

        return Ok(inactive);
    }

    private DateTime? ParseNullableDateTime(string value)
    {
        if (DateTime.TryParse(value, out var result))
            return result;
        return null;
    }



    // Remove Copilot license from user
    [Authorize]
    [HttpPost("remove-license/{userId}")]
    public async Task<IActionResult> RemoveCopilotLicense(string userId)
    {

        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>(),
            RemoveLicenses = new List<Guid?>() { skuGuid }
        };

        await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);
        return Ok(new { userId, removed = true });

    }

    [Authorize]
    [HttpPost("remove-license-by-email/{userEmail}")]
    public async Task<IActionResult> RemoveCopilotLicenseByEmail(string userEmail)
    {
        try
        {
            // Step 1: Get the user by email
            var user = await _graphClient.Users[userEmail]
                                         .GetAsync(config => {
                                             config.QueryParameters.Select = new[] { "id" };
                                         });

            if (user == null || string.IsNullOrEmpty(user.Id))
            {
                return NotFound(new { message = $"User with email {userEmail} not found." });
            }

            // Step 2: Prepare the license removal request
            var skuGuid = Guid.Parse(_copilotSkuId);

            var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
            {
                AddLicenses = new List<AssignedLicense>(),
                RemoveLicenses = new List<Guid?>() { skuGuid }
            };

            // Step 3: Remove the license using the user ID
            await _graphClient.Users[user.Id].AssignLicense.PostAsync(requestBody);

            return Ok(new { userEmail, removed = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while removing the license.", error = ex.Message });
        }
    }


    [Authorize]
    [HttpPost("assign-license/{userId}")]
    public async Task<IActionResult> AssignCopilotLicense(string userId)
    {
        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>
        {
            new AssignedLicense
            {
                SkuId = skuGuid
            }
        },
            RemoveLicenses = new List<Guid?>()
        };

        await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);
        return Ok(new { userId, assigned = true });
    }

    [Authorize]
    [HttpPost("assign-license-by-email/{userEmail}")]
    public async Task<IActionResult> AssignCopilotLicenseByEmail(string userEmail)
    {
        try
        {
            // Step 1: Get the user by email and retrieve their ID
            var user = await _graphClient.Users[userEmail]
                                         .GetAsync(config =>
                                         {
                                             config.QueryParameters.Select = new[] { "id" };
                                         });

            if (user == null || string.IsNullOrEmpty(user.Id))
            {
                return NotFound(new { message = $"User with email {userEmail} not found." });
            }

            // Step 2: Prepare the license assignment request
            var skuGuid = Guid.Parse(_copilotSkuId);

            var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
            {
                AddLicenses = new List<AssignedLicense>
            {
                new AssignedLicense
                {
                    SkuId = skuGuid
                }
            },
                RemoveLicenses = new List<Guid?>()
            };

            // Step 3: Assign the license using the user ID
            await _graphClient.Users[user.Id].AssignLicense.PostAsync(requestBody);

            return Ok(new { userEmail, assigned = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while assigning the license.", error = ex.Message });
        }
    }



    [Authorize]
    // 4. Usage details by app
    [HttpGet("usage-details")]
    public async Task<IActionResult> GetCopilotUsageDetails()
    {
        var usageList = await GetCopilotUsageReport();
        return Ok(usageList);
    }

    // --- Helper methods ---

    private async Task<HashSet<string>> GetActiveCopilotUserPrincipalNames()
    {
        var usageList = await GetCopilotUsageReport();
        var upns = new HashSet<string>();
        foreach (dynamic record in usageList)
        {
            var upn = record?.UserPrincipalName as string;
            if (!string.IsNullOrEmpty(upn))
                upns.Add(upn.ToLower());
        }
        return upns;
    }

    private async Task<List<dynamic>> GetCopilotUsageReport()
    {
        // Get access token
        var token = await _credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var url = "https://graph.microsoft.com/beta/reports/getOffice365CopilotUserDetail(period='D30')";
        var response = await httpClient.GetAsync(url);


        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // Log or return this errorContent for troubleshooting
            return new List<dynamic>();
        }


        var stream = await response.Content.ReadAsStreamAsync();
        var usageList = new List<dynamic>();
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>();
            foreach (var record in records)
            {
                usageList.Add(record);
            }
        }
        return usageList;
    }
}
