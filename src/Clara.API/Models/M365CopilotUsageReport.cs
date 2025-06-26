namespace Clara.API.Models;

public class M365CopilotUsageReport
{
    public required string UserId { get; set; }
    public required string UserDisplayName { get; set; }
    public string? UserPrincipalName { get; set; }

    public string? UserDepartment { get; set; }

    public DateTime? LastActivityDate { get; set; }
    public DateTime? CopilotChatLastActivityDate { get; set; }
    public DateTime? MicrosoftTeamsCopilotLastActivityDate { get; set; }
    public DateTime? WordCopilotLastActivityDate { get; set; }
    public DateTime? ExcelCopilotLastActivityDate { get; set; }
    public DateTime? PowerPointCopilotLastActivityDate { get; set; }
    public DateTime? OutlookCopilotLastActivityDate { get; set; }
    public DateTime? OneNoteCopilotLastActivityDate { get; set; }
    public DateTime? LoopCopilotLastActivityDate { get; set; }
}