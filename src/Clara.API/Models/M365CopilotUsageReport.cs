namespace Clara.API.Controllers;

public class M365CopilotUsageReport
{
    public string UserId { get; set; }
    public string UserDisplayName { get; set; }
    public string UserPrincipalName { get; set; }
    public dynamic LastActivityDate { get; set; }
    public dynamic CopilotChatLastActivityDate { get; set; }
    public dynamic MicrosoftTeamsCopilotLastActivityDate { get; set; }
    public dynamic WordCopilotLastActivityDate { get; set; }
    public dynamic ExcelCopilotLastActivityDate { get; set; }
    public dynamic PowerPointCopilotLastActivityDate { get; set; }
    public dynamic OutlookCopilotLastActivityDate { get; set; }
    public dynamic OneNoteCopilotLastActivityDate { get; set; }
    public dynamic LoopCopilotLastActivityDate { get; set; }
}