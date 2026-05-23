# LAB: Deploy Clara - M365 Copilot License Management Agent

## Lab Information

**Duration**: 3-4 hours  
**Level**: Intermediate  
**Prerequisites**: Azure subscription, M365 Global Admin access, Microsoft Copilot Studio access

## Lab Objectives

After completing this lab, you will be able to:
- Configure Azure AD applications for OAuth authentication
- Deploy REST APIs to Azure App Service  
- Create SharePoint lists for data management
- Configure and test Copilot Studio agents
- Integrate multiple Microsoft 365 services

## Lab Scenario

Your organization needs to optimize M365 Copilot license management. You will deploy Clara, an AI agent that automates license monitoring, waitlist management, and user communication.

---

# Exercise 1: Configure Azure AD Applications (45 minutes)

## Overview
Create two Azure AD app registrations: one for the API backend and one for the Copilot Studio client.

## Task 1: Create API App Registration

1. Open **Azure Portal** → **Azure Active Directory** → **App registrations**
2. Click **+ New registration**
3. Enter:
   - Name: `Clara Copilot Agent - API`
   - Account type: **Single tenant**
4. Click **Register**
5. **Record**: Application ID and Tenant ID

## Task 2: Add API Permissions

1. Select **API permissions** → **+ Add a permission**
2. Choose **Microsoft Graph** → **Application permissions**
3. Add these permissions:
   - `Directory.Read.All`
   - `Directory.ReadWrite.All`
   - `Reports.Read.All`
   - `User.Read.All`
4. Click **Grant admin consent**

## Task 3: Create Client Secret

1. Go to **Certificates & secrets** → **+ New client secret**
2. Description: `Clara API Secret`
3. Expiry: **6 months**
4. Click **Add**
5. **Copy the secret value immediately** (you won't see it again!)

## Task 4: Expose the API

1. Go to **Expose an API** → **Add** (Application ID URI)
2. Accept default: `api://[client-id]` → **Save**
3. Click **+ Add a scope**:
   - Scope name: `access_as_user`
   - Who can consent: **Admins and users**
   - Display name: `Access Clara Copilot Agent API`
   - Description: `Allows access to Clara API`
4. Click **Add scope**

## Task 5: Create Copilot Studio App Registration

1. **App registrations** → **+ New registration**
2. Enter:
   - Name: `Clara Copilot Agent - Copilot Studio`
   - Account type: **Single tenant**
3. Click **Register**
4. **Record**: Application ID

## Task 6: Add Delegated Permissions

1. **API permissions** → **+ Add a permission**
2. **APIs my organization uses** → Search for `Clara Copilot Agent - API`
3. Select **Delegated permissions** → Check `access_as_user`
4. Click **Add permissions** → **Grant admin consent**

## Task 7: Create Copilot Client Secret

1. **Certificates & secrets** → **+ New client secret**
2. Description: `Clara Copilot Studio Secret`
3. Click **Add**
4. **Copy the secret value**

## Task 8: Add Redirect URI

1. Go to **Authentication** → **+ Add a platform** → **Web**
2. Add Redirect URI:
   ```
   https://global.consent.azure-apim.net/redirect/clara-20apis-5fe4b1fa84f5c2dfb1-5f0af71351437b9f07
   ```
3. Enable **Access tokens** under Implicit grant
4. Click **Save**

✅ **Checkpoint**: Verify you have recorded:
- API Client ID and Secret
- Copilot Studio Client ID and Secret
- Tenant ID

---

# Exercise 2: Deploy REST API to Azure (45 minutes)

## Overview
Deploy the Clara REST API to Azure App Service for backend operations.

## Task 1: Create Web App

1. Azure Portal → **Create a resource**
2. Search for **Web App** → **Create**
3. Configure:
   - Resource Group: **Create new** → `clara-copilot-rg`
   - Name: `clara-api-[yourname]` (globally unique)
   - Runtime: **.NET 8 (LTS)**
   - OS: **Linux**
   - Region: Choose nearest
4. Click **Review + create** → **Create**
5. After deployment: **Go to resource**

## Task 2: Configure Environment Variables

1. Select **Environment variables** → **App settings** → **Advanced edit**
2. Paste this JSON (replace placeholders with your values):

```json
[
  {
    "name": "ASPNETCORE_ENVIRONMENT",
    "value": "Development",
    "slotSetting": false
  },
  {
    "name": "AzureAd:Audience",
    "value": "api://<YOUR-API-CLIENT-ID>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:Authority",
    "value": "https://login.microsoftonline.com/<YOUR-TENANT-ID>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:ClientId",
    "value": "<YOUR-API-CLIENT-ID>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:ClientSecret",
    "value": "<YOUR-API-CLIENT-SECRET>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:Instance",
    "value": "https://login.microsoftonline.com/",
    "slotSetting": false
  },
  {
    "name": "AzureAd:TenantId",
    "value": "<YOUR-TENANT-ID>",
    "slotSetting": false
  },
  {
    "name": "CopilotSkuId",
    "value": "639dec6b-bb19-468b-871c-c5c441c4b0cb",
    "slotSetting": false
  },
  {
    "name": "M365CopilotDashboardUrl",
    "value": "https://graph.microsoft.com/beta/reports/getMicrosoft365CopilotUsageUserDetail(period='D30')?$format=application/json",
    "slotSetting": false
  },
  {
    "name": "CopilotGroupName",
    "value": "M365 Copilot Users",
    "slotSetting": false
  }
]
```

3. Click **OK** → **Apply** → **Confirm**

## Task 3: Enable Deployment

1. **Deployment Center** → Source: **External Git**
2. If prompted, enable **SCM Basic Authentication**:
   - Click **Enable here** → **Configuration**
   - Enable **SCM Basic Auth**
   - Click **Save**
3. **Overview** → **Restart** the app
4. Return to **Deployment Center**

## Task 4: Configure Git Deployment

1. Enter:
   - Repository: `https://github.com/luishdemetrio/clara-copilot-agent.git`
   - Branch: `main`
   - Type: **Public**
2. Click **Save**
3. Monitor deployment in **Deployment Center** (5-10 minutes)

## Task 5: Verify Deployment

1. Click **Browse** (top of page)
2. Append `/swagger` to URL
3. Verify Swagger UI loads with API endpoints

✅ **Checkpoint**: Swagger UI is accessible at `https://[your-app].azurewebsites.net/swagger`

---

# Exercise 3: Create SharePoint Waitlist (30 minutes)

## Overview
Create a SharePoint list to track users waiting for licenses.

## Task 1: Create List

1. Navigate to your **SharePoint site**
2. **New** → **List** → **Blank list**
3. Name: `M365 Copilot Waitlist`
4. Click **Create**

## Task 2: Hide Title Column

1. Click **Title** column header
2. **Column settings** → **Hide this column**

## Task 3: Add Columns

**Column 1: UserEmail**
1. **+ Add column** → **Single line of text**
2. Name: `UserEmail`
3. Click **Save**

**Column 2: User Waiting License**
1. **+ Add column** → **Person**
2. Name: `User Waiting License`
3. Click **Save**

**Column 3: Status**
1. **+ Add column** → **Choice**
2. Name: `Status`
3. Choices: `Requested`, `Approved`
4. Default: `Requested`
5. Click **Save**

**Column 4: Approved By**
1. **+ Add column** → **Person**
2. Name: `Approved By`
3. Click **Save**

✅ **Checkpoint**: List has 4 visible columns (UserEmail, User Waiting License, Status, Approved By)

---

# Exercise 4: Import and Configure Clara Agent (60 minutes)

## Overview
Import Clara into Copilot Studio and connect all services.

## Task 1: Import Solution

1. Navigate to **[Copilot Studio](https://copilotstudio.microsoft.com)**
2. **Menu (...)** → **Solutions**
3. **Import solution** → **Browse**
4. Download and select: [ClaraCopilotAgent_2_0_0_3.zip](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/agent/ClaraCopilotAgent_2_0_0_3.zip)
5. Click **Next** → **Next** → **Import**
6. Wait for completion (3-5 minutes)

## Task 2: Configure Custom Connector

1. Open new tab: **[Power Automate](https://make.powerautomate.com)**
2. **More** → **Custom connectors** → Find **Clara API**
3. Click **Edit** (pencil icon)
4. **Security** tab → **Edit**
5. Set:
   - Identity Provider: **Generic OAuth 2**
   - Client Secret: Your **Copilot Studio Client Secret**
6. Click **Update connector**

## Task 3: Test Connector

1. **Test** tab → **+ New connection**
2. Sign in with M365 credentials
3. Find **GetCopilotLicenseAvailability** operation
4. Click **Test operation**
5. Verify JSON response with license counts

## Task 4: Connect Clara Agent

1. Return to **Copilot Studio** → Open **Clara** agent
2. In **Test panel**, enter:
   ```
   How many Copilot licenses are available?
   ```
3. Click **Connect** → **Connect** again
4. Sign in with M365 credentials

**If AADSTS50011 error occurs:**
1. Azure Portal → **App registrations** → **Clara Copilot Agent - Copilot Studio**
2. **Authentication** → Add the redirect URI from error
3. **Save** → Retry in Copilot Studio

5. Click **Retry**
6. Verify response with license information

## Task 5: Configure SharePoint Connection

1. Clara agent → **Tools** tab → **Get Copilot Waitlist Users**
2. Set:
   - Site Address: Your SharePoint site URL
   - List Name: `M365 Copilot Waitlist`
3. Click **Save**

## Task 6: Test SharePoint Integration

1. Test panel, enter:
   ```
   What's the current waitlist status?
   ```
2. Click **Connect**
3. **Manage connections** → **Connect** SharePoint
4. Sign in → **Submit**
5. Click **Retry**
6. Verify response about waitlist

## Task 7: Configure Update Flow

1. **Topics** tab → **Update the waitlist status to approved**
2. Click **View flow details**
3. **Designer** tab
4. In **Update item** action:
   - Site Address: Your SharePoint URL
   - List Name: `M365 Copilot Waitlist`
5. Click **Publish**

✅ **Checkpoint**: All connections work, Clara responds to queries

---

# Exercise 5: End-to-End Testing (30 minutes)

## Task 1: Add Test User to Waitlist

1. SharePoint → **M365 Copilot Waitlist** → **+ New**
2. Fill in:
   - UserEmail: test@yourdomain.com
   - User Waiting License: Select a user
   - Status: Requested
3. Click **Save**

## Task 2: Query Waitlist

In Copilot Studio test panel:
```
Show me users on the waitlist
```
Verify the test user appears.

## Task 3: Check License Availability

```
How many licenses are available?
```
Verify accurate count is returned.

## Task 4: Test Additional Queries

Try these queries:
```
Who has the most Copilot usage?
```
```
List all Copilot license holders
```

## Task 5: Publish Clara (Optional)

1. Click **Publish** → **Publish**
2. Wait for completion
3. Configure deployment channels as needed

✅ **Checkpoint**: Clara responds accurately to all queries

---

# Lab Cleanup (Optional)

If this is a test environment:

1. **Azure Portal**:
   - Delete Resource Group `clara-copilot-rg`
   - Delete both App Registrations

2. **SharePoint**:
   - Delete `M365 Copilot Waitlist` list

3. **Copilot Studio**:
   - Delete Clara solution

---

# Lab Summary

## What You Accomplished

✅ Configured Azure AD OAuth 2.0 authentication  
✅ Deployed a .NET REST API to Azure App Service  
✅ Created and configured SharePoint lists  
✅ Imported and configured a Copilot Studio agent  
✅ Integrated multiple Microsoft 365 services  
✅ Tested the complete solution end-to-end

## Key Skills Learned

- Azure AD application registration and OAuth flows
- Azure App Service deployment and configuration
- SharePoint list design and management
- Custom connector configuration in Power Automate
- Copilot Studio agent development and testing

## Next Steps

1. Customize Clara's topics for your organization
2. Add email/Teams notifications
3. Create usage reports and dashboards
4. Deploy to production environment
5. Onboard your IT team

## Resources

- **GitHub**: [Clara Repository](https://github.com/luishdemetrio/clara-copilot-agent)
- **Docs**: [Copilot Studio](https://learn.microsoft.com/microsoft-copilot-studio)
- **Support**: [GitHub Discussions](https://github.com/luishdemetrio/clara-copilot-agent/discussions)

---

# Troubleshooting Guide

| Issue | Solution |
|-------|----------|
| AADSTS50011 error | Add redirect URI to app registration's Authentication |
| 401 Unauthorized | Grant admin consent for API permissions |
| SharePoint connection fails | Verify site permissions and list name |
| Deployment fails | Check App Service logs and environment variables |
| Connector test fails | Verify API is accessible via Swagger UI |

---

**Congratulations!** You have successfully completed the Clara deployment lab.

For questions or feedback, visit [GitHub Discussions](https://github.com/luishdemetrio/clara-copilot-agent/discussions).
