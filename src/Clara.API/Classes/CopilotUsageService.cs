using Azure.Identity;
using Clara.API.Models;
using Microsoft.Graph;
using System.Globalization;
using System.Net.Http.Headers;
using CsvHelper;
using Clara.API.Interfaces;

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

    public async Task<IEnumerable<M365CopilotUsageReport>> GetInactiveUsersAsync()
    {
        var users = await _graphClient.Users
            .GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Filter = $"assignedLicenses/any(x:x/skuId eq {_copilotSkuId})";
                requestConfig.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName" };
            });

        var usageList = await GetCopilotUsageReport();

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
                dynamic? usageRecord = null;
                usageDict.TryGetValue(u.UserPrincipalName!, out usageRecord);

                return new M365CopilotUsageReport
                {
                    UserId = u.Id!,
                    UserDisplayName = u.DisplayName!,
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

        return inactive;
    }

  

    // --- Helper methods ---

    private async Task<List<dynamic>> GetCopilotUsageReport()
    {
        var token = await _credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var response = await httpClient.GetAsync(_m365CopilotDashboardUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // Log or handle errorContent as needed
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

    private DateTime? ParseNullableDateTime(string value)
    {
        if (DateTime.TryParse(value, out var result))
            return result;
        return null;
    }
}
