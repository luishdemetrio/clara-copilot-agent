using Azure.Identity;
using Clara.API.Models;
using Microsoft.Graph;
using System.Net.Http.Headers;
using Clara.API.Interfaces;
using Newtonsoft.Json.Linq;

namespace Clara.API.Classes;

public class CopilotUsageService : ICopilotUsageService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ClientSecretCredential _credential;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _copilotSkuId;
    private readonly string _m365CopilotDashboardUrl;

    public CopilotUsageService(
        GraphServiceClient graphClient,
        ClientSecretCredential credential,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _graphClient = graphClient;
        _credential = credential;
        _httpClientFactory = httpClientFactory;
        _copilotSkuId = config["CopilotSkuId"]!;
        _m365CopilotDashboardUrl = config["M365CopilotDashboardUrl"]!;
    }

    public async Task<IEnumerable<M365CopilotUsageReport>> GetInactiveUsersAsync(int? days = null, int? topUsers = null)
    {
         // Fetch users with Copilot licenses
        var users = await _graphClient.Users
            .GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Filter = $"assignedLicenses/any(x:x/skuId eq {_copilotSkuId})";
                requestConfig.QueryParameters.Select = new[] { "id", "userPrincipalName"};
            });

        // Fetch Copilot usage report
        var usageList = await GetCopilotUsageReport();
        
        // Validate inputs
        if (users?.Value == null || usageList == null)
        {
            return Enumerable.Empty<M365CopilotUsageReport>();
        }

        
        // Build a HashSet of userPrincipalNames from the users list for fast lookup
        var userPrincipalNames = new HashSet<string>(
            users.Value.Select(u => u.UserPrincipalName!),
            StringComparer.OrdinalIgnoreCase
        );

        // Filter usageList to only those present in users list
        var filteredUsageList = usageList
            .Where(report => userPrincipalNames.Contains(report.UserPrincipalName!))
            .ToList();


        var joined = from usage in usageList
             join user in users.Value
             on usage.UserPrincipalName!.ToLowerInvariant() equals user.UserPrincipalName!.ToLowerInvariant()
             select new M365CopilotUsageReport
             {
                 UserId = user.Id!,
                 UserDisplayName = usage.UserDisplayName,
                 UserPrincipalName = usage.UserPrincipalName,
                 UserDepartment = usage.UserDepartment,
                 LastActivityDate = usage.LastActivityDate,
                 CopilotChatLastActivityDate = usage.CopilotChatLastActivityDate,
                 MicrosoftTeamsCopilotLastActivityDate = usage.MicrosoftTeamsCopilotLastActivityDate,
                 WordCopilotLastActivityDate = usage.WordCopilotLastActivityDate,
                 ExcelCopilotLastActivityDate = usage.ExcelCopilotLastActivityDate,
                 PowerPointCopilotLastActivityDate = usage.PowerPointCopilotLastActivityDate,
                 OutlookCopilotLastActivityDate = usage.OutlookCopilotLastActivityDate,
                 OneNoteCopilotLastActivityDate = usage.OneNoteCopilotLastActivityDate,
                 LoopCopilotLastActivityDate = usage.LoopCopilotLastActivityDate
             };



        // Order by LastActivityDate descending (nulls last)
        var ordered = joined.OrderByDescending(report => report.LastActivityDate ?? DateTime.MinValue);

        // Apply filtering and limiting in a single chain
        var result = ordered.AsEnumerable(); // Convert to IEnumerable early to avoid type issues

        if (days.HasValue)
        {
            result = result.Where(report =>
                report.LastActivityDate == null ||
                (
                    report.LastActivityDate is DateTime lastActivity &&
                    (DateTime.UtcNow.Subtract(lastActivity)).TotalDays >= days.Value
                ));
        }

        if (topUsers.HasValue && topUsers.Value > 0)
        {
            result = result.Take(topUsers.Value);
        }

        return result;
    }

  

    // --- Helper methods ---

   private async Task<List<M365CopilotUsageReport>> GetCopilotUsageReport()
{
    var token = await _credential.GetTokenAsync(
        new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));

    var httpClient = _httpClientFactory.CreateClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

    var usageList = new List<M365CopilotUsageReport>();
    string requestUrl = _m365CopilotDashboardUrl;

    while (!string.IsNullOrEmpty(requestUrl))
    {
        var response = await httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // Log or handle errorContent as needed
            return new List<M365CopilotUsageReport>();
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var json = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

        // The data is usually under the "value" property
        var records = json["value"];
        if (records != null)
        {
            foreach (var record in records)
            {
                // Map JSON to your class
                var usageReport = new M365CopilotUsageReport
                        {
                            UserId = string.Empty, // I will populate it later
                            UserDisplayName = (string)record["displayName"]!,
                            UserPrincipalName = (string)record["userPrincipalName"]!,
                            UserDepartment = (string)record["department"]!,
                            LastActivityDate = ParseNullableDateTime(record["lastActivityDate"]!),
                            CopilotChatLastActivityDate = ParseNullableDateTime(record["copilotChatLastActivityDate"]!),
                            MicrosoftTeamsCopilotLastActivityDate = ParseNullableDateTime(record["microsoftTeamsCopilotLastActivityDate"]!),
                            WordCopilotLastActivityDate = ParseNullableDateTime(record["wordCopilotLastActivityDate"]!),
                            ExcelCopilotLastActivityDate = ParseNullableDateTime(record["excelCopilotLastActivityDate"]!),
                            PowerPointCopilotLastActivityDate = ParseNullableDateTime(record["powerPointCopilotLastActivityDate"]!),
                            OutlookCopilotLastActivityDate = ParseNullableDateTime(record["outlookCopilotLastActivityDate"]!),
                            OneNoteCopilotLastActivityDate = ParseNullableDateTime(record["oneNoteCopilotLastActivityDate"]!),
                            LoopCopilotLastActivityDate = ParseNullableDateTime(record["loopCopilotLastActivityDate"]!)
                        };

                usageList.Add(usageReport);
            }
        }

        // Check for pagination
        requestUrl = json["@odata.nextLink"]?.ToString()!;
    }

    return usageList;
}


    private DateTime? ParseNullableDateTime(JToken  token)
    {
      
        var str = (string)token!;
        if (DateTime.TryParse(str, out var dt))
            return dt;

        return null;

    }
}
