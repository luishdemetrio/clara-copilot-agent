# Install the Microsoft.Graph.Beta module if not already installed
if (-not (Get-Module -Name Microsoft.Graph.Beta -ListAvailable)) {
    Install-Module -Name Microsoft.Graph.Beta -Force -AllowClobber
}

# Import the Microsoft.Graph.Beta module
Import-Module Microsoft.Graph.Beta

# Connect to Microsoft Graph
Connect-MgGraph -Scopes "Reports.Read.All" 
# Get Copilot user usage details
$CopilotUsageDetails = Invoke-MgGraphRequest -Method GET -Uri "https://graph.microsoft.com/beta/reports/getMicrosoft365CopilotUsageUserDetail(period='D180')"

# Create an array to store the usage data
$UsageData = @()

#get mydocuments path
$mydocumentsPath = [System.Environment]::GetFolderPath('MyDocuments')
# Set path to the CSV file NOTE: CHANGE THIS TO YOUR PREFERED PATH
$csvPath = Join-Path $mydocumentsPath -childpath "\CopilotUsageDetails.csv"

# Loop through each user and extract the usage details
foreach ($User in $CopilotUsageDetails.value) {
    $UsageData += [PSCustomObject]@{
        reportRefreshDate = $User.reportRefreshDate
        UserPrincipalName = $User.UserPrincipalName
        DisplayName = $User.DisplayName
        LastActivityDate = $User.LastActivityDate
        copilotChatLastActivityDate = $User.copilotChatLastActivityDate
        microsoftTeamsCopilotLastActivityDate = $user.microsoftTeamsCopilotLastActivityDate
        wordCopilotLastActivityDate = $user.wordCopilotLastActivityDate
        excelCopilotLastActivityDate = $user.excelCopilotLastActivityDate
        powerPointCopilotLastActivityDate = $user.powerPointCopilotLastActivityDate
        outlookCopilotLastActivityDate = $user.outlookCopilotLastActivityDate
        oneNoteCopilotLastActivityDate = $user.oneNoteCopilotLastActivityDate
        loopCopilotLastActivityDate = $user.loopCopilotLastActivityDate   
    }
}

Write-Host ("{0} usage data records processed" -f $UsageData.count)

# Check if the CSV file already exists
if (Test-Path $csvPath) {
    # Read the existing data from the CSV file
    $ExistingData = Import-Csv -Path $csvPath

    # Combine the existing data with the new data
    $CombinedData = $ExistingData + $UsageData

    # Remove duplicates based on UserPrincipalName and LastactivityDate
    $UniqueData = $CombinedData | Sort-Object UserPrincipalName, LastActivityDate -Unique

    # Export the unique data to the CSV file
    $UniqueData | Export-Csv -Path $csvPath -NoTypeInformation
} else {
    # Create a new CSV file with the new data
    $UsageData | Export-Csv -Path $csvPath -NoTypeInformation
}
Write-Host "Copilot user usage details have been exported to" $csvPath


