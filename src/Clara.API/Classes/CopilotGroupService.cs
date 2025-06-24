using Clara.API.Interfaces;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Clara.API.Classes;

public class CopilotGroupService : ICopilotGroupService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IConfiguration _config;

    public CopilotGroupService(GraphServiceClient graphClient, IConfiguration config)
    {
        _graphClient = graphClient;
        _config = config;
    }

    public async Task<bool> AddUserToGroupAsync(string userEmail)
    {
        var groupId = await GetGroupIdAsync();
        if (string.IsNullOrEmpty(groupId))
            return false;

        var user = await _graphClient.Users[userEmail]
            .GetAsync(config => { config.QueryParameters.Select = new[] { "id" }; });

        if (user == null || string.IsNullOrEmpty(user.Id))
            return false;

        await _graphClient.Groups[groupId].Members.Ref.PostAsync(new ReferenceCreate
        {
            OdataId = $"https://graph.microsoft.com/v1.0/directoryObjects/{user.Id}"
        });

        return true;
    }

    public async Task<bool> RemoveUserFromGroupAsync(string userEmail)
    {
        var groupId = await GetGroupIdAsync();
        if (string.IsNullOrEmpty(groupId))
            return false;

        var user = await _graphClient.Users[userEmail]
            .GetAsync(config => { config.QueryParameters.Select = new[] { "id" }; });

        if (user == null || string.IsNullOrEmpty(user.Id))
            return false;

        await _graphClient.Groups[groupId].Members[user.Id].Ref.DeleteAsync();
        return true;
    }

    private async Task<string?> GetGroupIdAsync()
    {
        var groupId = _config["CopilotGroupId"];
        if (!string.IsNullOrEmpty(groupId))
            return groupId;

        var groupName = _config["CopilotGroupName"];
        if (string.IsNullOrEmpty(groupName))
            return null;

        var groups = await _graphClient.Groups
            .GetAsync(config =>
            {
                config.QueryParameters.Filter = $"displayName eq '{groupName}'";
                config.QueryParameters.Select = new[] { "id" };
            });

        return groups?.Value?.FirstOrDefault()?.Id;
    }
}
