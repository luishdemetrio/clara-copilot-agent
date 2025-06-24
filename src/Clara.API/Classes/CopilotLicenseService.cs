using Clara.API.Interfaces;
using Clara.API.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Clara.API.Classes;

public class CopilotLicenseService : ICopilotLicenseService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IConfiguration _config;
    private readonly string _copilotSkuId;

    public CopilotLicenseService(GraphServiceClient graphClient, IConfiguration config)
    {
        _graphClient = graphClient;
        _config = config;
        _copilotSkuId = _config["CopilotSkuId"]!;
    }

    public async Task<bool> AssignLicenseByEmailAsync(string userEmail)
    {
        // Get user by email
        var user = await _graphClient.Users[userEmail]
            .GetAsync(config => { config.QueryParameters.Select = new[] { "id" }; });

        if (user == null || string.IsNullOrEmpty(user.Id))
            return false;

        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>
            {
                new AssignedLicense { SkuId = skuGuid }
            },
            RemoveLicenses = new List<Guid?>()
        };

        await _graphClient.Users[user.Id].AssignLicense.PostAsync(requestBody);
        return true;
    }

    public async Task<bool> RemoveLicenseByEmailAsync(string userEmail)
    {
        // Get user by email
        var user = await _graphClient.Users[userEmail]
            .GetAsync(config => { config.QueryParameters.Select = new[] { "id" }; });

        if (user == null || string.IsNullOrEmpty(user.Id))
            return false;

        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>(),
            RemoveLicenses = new List<Guid?>() { skuGuid }
        };

        await _graphClient.Users[user.Id].AssignLicense.PostAsync(requestBody);
        return true;
    }

    public async Task<LicenseCountsDto> GetLicenseCountsAsync()
    {
        var skus = await _graphClient.SubscribedSkus.GetAsync();

        var copilotSku = skus?.Value?
            .FirstOrDefault(sku =>
                sku.SkuId.HasValue &&
                sku.SkuId.Value.ToString().Equals(_copilotSkuId, StringComparison.OrdinalIgnoreCase)
            );

        if (copilotSku == null)
            return new LicenseCountsDto();

        var total = copilotSku.PrepaidUnits?.Enabled ?? 0;
        var assigned = copilotSku.ConsumedUnits ?? 0;
        var available = total - assigned;

        return new LicenseCountsDto
        {
            TotalLicenses = total,
            AssignedLicenses = assigned,
            AvailableLicenses = available
        };
    }

    public async Task<bool> AssignLicenseByIdAsync(string userId)
    {
        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>
        {
            new AssignedLicense { SkuId = skuGuid }
        },
            RemoveLicenses = new List<Guid?>()
        };

        await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);
        return true;
    }

    public async Task<bool> RemoveLicenseByIdAsync(string userId)
    {
        var skuGuid = Guid.Parse(_copilotSkuId);

        var requestBody = new Microsoft.Graph.Users.Item.AssignLicense.AssignLicensePostRequestBody
        {
            AddLicenses = new List<AssignedLicense>(),
            RemoveLicenses = new List<Guid?>() { skuGuid }
        };

        await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);
        return true;
    }

}
