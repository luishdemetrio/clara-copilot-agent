namespace Clara.API.Interfaces;

public interface ICopilotGroupService
{
    Task<bool> AddUserToGroupAsync(string userEmail);
    Task<bool> RemoveUserFromGroupAsync(string userEmail);
}

