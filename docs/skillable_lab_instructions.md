# Hands-On Lab: Deploy and Configure Clara - M365 Copilot License Management Agent

## Lab Overview

In this hands-on lab, you will deploy and configure **Clara**, an intelligent AI agent built on Microsoft Copilot Studio that automates M365 Copilot license management for enterprises. Clara helps IT teams monitor license usage, manage waitlists, and optimize license allocation to ensure maximum ROI on M365 Copilot investments.

### What You Will Learn

By the end of this lab, you will be able to:
- Register and configure Azure AD applications for API authentication
- Deploy a REST API to Azure App Service
- Create and configure SharePoint lists for license management
- Import and configure a Copilot Studio agent
- Test and validate the complete solution

### Prerequisites

- Active Azure subscription with appropriate permissions
- Microsoft 365 tenant with Global Administrator or Application Administrator role
- Access to Microsoft Copilot Studio
- Basic understanding of Azure AD, REST APIs, and SharePoint
- Familiarity with Microsoft 365 administration

### Estimated Time to Complete

**3-4 hours**

### Difficulty Level

**Intermediate**

---

## Lab Architecture

The Clara solution consists of the following components:

```mermaid
graph TB
    A[Clara Agent in Copilot Studio] --> B[Microsoft Graph APIs]
    A --> C[SharePoint Lists]
    A --> D[Custom REST API]
    
    B --> E[M365 Copilot Usage Dashboard]
    C --> F[Waitlist Management]
    D --> G[License Operations]
    
    E --> H[Usage Analytics]
    F --> I[User Tracking]
    G --> J[License Assignment]
```

---

## Exercise 1: Azure Application Registration

In this exercise, you will create and configure two Azure AD app registrations: one for the REST API (server) and one for Copilot Studio (client). This separation provides better security and token scoping.

### Task 1.1: Create API App Registration

1. Navigate to the [Azure Portal](https://portal.azure.com)
2. Go to **Azure Active Directory** > **App registrations**
3. Click **+ New registration**

   ![New Registration](images/az01.png)

4. Configure the application:
   - **Name**: `Clara Copilot Agent - API`
   - **Supported account types**: Single tenant (Accounts in this organizational directory only)
   - **Redirect URI**: Leave blank for now
5. Click **Register**

   ![Register API App](images/az02.png)

6. **Record the following values** (you'll need them later):
   - Application (client) ID
   - Directory (tenant) ID

### Task 1.2: Configure API Permissions

1. In your app registration, select **API permissions**
2. Click **+ Add a permission**
3. Select **Microsoft Graph**

   ![Microsoft Graph](images/az03.png)

4. Choose **Application permissions**
5. Add the following permissions:
   - `Directory.Read.All`
   - `Directory.ReadWrite.All`
   - `Reports.Read.All`
   - `User.Read.All`
6. Click **Add permissions**

   ![Add Permissions](images/az04.png)

7. Click **Grant admin consent for [Your Organization]**
8. Confirm by clicking **Yes**

   ![Grant Consent](images/az05.png)

### Task 1.3: Generate Client Secret

1. Go to **Certificates & secrets**
2. Click **+ New client secret**
3. Add a description: `Clara API Secret`
4. Choose an expiry period (e.g., 6 months or 1 year)
5. Click **Add**

   ![Create Secret](images/az08.png)

6. **Important**: Copy the secret **Value** immediately - you won't be able to see it again!

   ![Copy Secret](images/az09.png)

### Task 1.4: Expose the API

1. Go to **Expose an API**
2. Click **Add** next to Application ID URI
3. Accept the default value (api://[client-id]) and click **Save**

   ![Application ID URI](images/az06.png)

4. Click **+ Add a scope**
5. Configure the scope:
   - **Scope name**: `access_as_user`
   - **Who can consent**: Admins and users
   - **Admin consent display name**: `Access Clara Copilot Agent API`
   - **Admin consent description**: `Allows the app to access Clara Copilot Agent API on behalf of the user`
   - **User consent display name**: `Access Clara Copilot Agent API`
   - **User consent description**: `Allows the app to access Clara Copilot Agent API on your behalf`
   - **State**: Enabled
6. Click **Add scope**

   ![Add Scope](images/az07.png)

### Task 1.5: Create Copilot Studio App Registration

1. Return to **Azure Active Directory** > **App registrations**
2. Click **+ New registration**

   ![New Registration](images/az01.png)

3. Configure the application:
   - **Name**: `Clara Copilot Agent - Copilot Studio`
   - **Supported account types**: Single tenant
   - **Redirect URI**: Leave blank for now
4. Click **Register**

   ![Register Copilot App](images/azcs01.png)

5. **Record the Application (client) ID**

### Task 1.6: Configure Copilot Studio API Permissions

1. Go to **API permissions**
2. Click **+ Add a permission**
3. Select **APIs my organization uses**
4. Search for and select **Clara Copilot Agent - API**

   ![Select Clara API](images/azcs02.png)

5. Select **Delegated permissions**
6. Check the `access_as_user` scope
7. Click **Add permissions**

   ![Add Scope Permission](images/azcs03.png)

8. Click **Grant admin consent for [Your Organization]**
9. Confirm by clicking **Yes**

   ![Grant Consent](images/azcs04.png)

### Task 1.7: Generate Copilot Studio Client Secret

1. Go to **Certificates & secrets**
2. Click **+ New client secret**
3. Add a description: `Clara Copilot Studio Secret`
4. Choose an expiry period
5. Click **Add**

   ![Create Copilot Secret](images/azcs05.png)

6. **Important**: Copy the secret **Value** immediately

   ![Copy Copilot Secret](images/azcs06.png)

### Task 1.8: Configure Authentication

1. Go to **Authentication**
2. Under **Platform configurations**, click **+ Add a platform**
3. Select **Web**
4. Add the following Redirect URI:
   ```
   https://global.consent.azure-apim.net/redirect/clara-20apis-5fe4b1fa84f5c2dfb1-5f0af71351437b9f07
   ```
5. Under **Implicit grant and hybrid flows**, check:
   - **Access tokens (used for implicit flows)**
6. Click **Configure**

   ![Configure Authentication](images/azcs07.png)

7. Click **Save**

### Knowledge Check 1

Before proceeding, verify you have recorded the following:
- [ ] Clara API - Application (client) ID
- [ ] Clara API - Client Secret Value
- [ ] Copilot Studio - Application (client) ID
- [ ] Copilot Studio - Client Secret Value
- [ ] Tenant ID

---

## Exercise 2: Deploy REST API to Azure App Service

In this exercise, you will deploy the Clara REST API to Azure App Service, which provides the backend services for license management operations.

### Task 2.1: Create Azure Web App

1. In the [Azure Portal](https://portal.azure.com), click **Create a resource**

   ![Create Resource](images/as01.png)

2. Search for **Web App** and click **Create**

   ![Create Web App](images/as02.png)

3. Configure the web app:
   - **Subscription**: Select your subscription
   - **Resource Group**: Create new: `clara-copilot-rg`
   - **Name**: `clara-copilot-api-[yourname]` (must be globally unique)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Linux
   - **Region**: Select the region closest to you

   ![Web App Basics](images/as03.png)

4. Click **Review + create**

   ![Review and Create](images/as04.png)

5. Click **Create**

   ![Create Web App](images/as05.png)

6. Wait for deployment to complete, then click **Go to resource**

   ![Go to Resource](images/as06.png)

### Task 2.2: Configure Application Settings

1. In your Web App, select **Environment variables** from the left menu
2. Under **App settings**, click **Advanced edit**
3. Replace the content with the following JSON (update the placeholder values with your actual values):

```json
[
  {
    "name": "ASPNETCORE_ENVIRONMENT",
    "value": "Development",
    "slotSetting": false
  },
  {
    "name": "AzureAd:Audience",
    "value": "api://<Clara-API-Client-ID>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:Authority",
    "value": "https://login.microsoftonline.com/<YOUR-TENANT-ID>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:ClientId",
    "value": "<Clara-API-Client-ID>",
    "slotSetting": false
  },
  {
    "name": "AzureAd:ClientSecret",
    "value": "<Clara-API-Client-Secret>",
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

4. Click **OK**

   ![Advanced Edit](images/as07.png)

5. Click **Apply**
6. Click **Confirm** when prompted

   ![Apply Settings](images/as08.png)

### Task 2.3: Configure Deployment

1. In the left menu, select **Deployment Center**
2. Under **Source**, select **External Git**
3. Check if **SCM Basic Auth** is enabled
   - If not, click the **Enable here** link

   ![Enable Auth](images/as09.png)

4. You'll be redirected to **Configuration**
   - Enable **SCM Basic Auth Publishing Credentials**
   - Click **Save**

   ![Enable SCM Auth](images/as10.png)

5. Return to **Overview** and click **Restart**

   ![Restart App Service](images/as11.png)

6. Go back to **Deployment Center**
7. Configure the repository:
   - **Repository**: `https://github.com/luishdemetrio/clara-copilot-agent.git`
   - **Branch**: `main`
   - **Repository Type**: Public
8. Click **Save**

   ![Configure Deployment](images/as12.png)

### Task 2.4: Monitor Deployment

1. In the **Deployment Center**, monitor the deployment logs
2. Wait for the deployment to complete (this may take 5-10 minutes)

   ![Monitor Deployment](images/as13.png)

3. Once complete, click **Browse** at the top of the page
4. Append `/swagger` to the URL to view the API documentation
   - Example: `https://clara-copilot-api-[yourname].azurewebsites.net/swagger`

   ![Swagger UI](images/as14.png)

### Knowledge Check 2

Verify your deployment:
- [ ] Web App is running
- [ ] Swagger UI is accessible
- [ ] API endpoints are visible in Swagger

---

## Exercise 3: Create SharePoint Waitlist

In this exercise, you will create a SharePoint list to manage users waiting for M365 Copilot licenses.

### Task 3.1: Create SharePoint List

1. Navigate to your SharePoint site (e.g., `https://yourtenant.sharepoint.com/sites/YourSite`)
2. Click **New** in the top navigation
3. Select **List**

   ![New List](images/sl01.png)

4. Select **Blank list**

   ![Blank List](images/sl02.png)

5. Configure the list:
   - **Name**: `M365 Copilot Waitlist`
   - **Description**: `Manages users waiting for M365 Copilot licenses`
6. Click **Create**

   ![Create List](images/sl03.png)

### Task 3.2: Configure List Columns

1. **Hide the Title column** (we won't use it):
   - Click the **Title** column header
   - Select **Column settings** > **Hide this column**

   ![Hide Title](images/sl07.png)

2. **Add UserEmail column**:
   - Click **+ Add column**

   ![Add Column](images/sl04.png)

   - Select **Single line of text**

   ![Text Column](images/sl05.png)

   - **Name**: `UserEmail`
   - **Description**: `User email address`
   - Click **Save**

   ![UserEmail Column](images/sl06.png)

3. **Add User Waiting License column**:
   - Click **+ Add column**
   - Select **Person**

   ![Person Column](images/sl08.png)

   - **Name**: `User Waiting License`
   - **Description**: `User waiting for license`
   - **Allow multiple selections**: No
   - Click **Save**

   ![User Column](images/sl09.png)

4. **Add Status column**:
   - Click **+ Add column**

   ![Add Column](images/sl10.png)

   - Select **Choice**

   ![Choice Column](images/sl11.png)

   - **Name**: `Status`
   - **Choices**: 
     - `Requested`
     - `Approved`
   - **Default value**: Requested
   - Click **Save**

   ![Status Column](images/sl12.png)

5. **Add Approved By column**:
   - Click **+ Add column**
   - Select **Person**

   ![Person Column](images/sl13.png)

   - **Name**: `Approved By`
   - **Description**: `Person who approved the license`
   - **Allow multiple selections**: No
   - Click **Save**

   ![Approved By Column](images/sl14.png)

### Task 3.3: Verify List Structure

Your list should now have the following columns:
- UserEmail (Text)
- User Waiting License (Person)
- Status (Choice)
- Approved By (Person)

![Final List Structure](images/sl15.png)

### Knowledge Check 3

Verify your SharePoint list:
- [ ] List is created with correct name
- [ ] All five columns are configured
- [ ] Title column is hidden
- [ ] You can add a test item to the list

---

## Exercise 4: Import Clara Agent to Copilot Studio

In this exercise, you will import the Clara agent into Microsoft Copilot Studio and configure it to work with your Azure resources.

### Task 4.1: Import Solution

1. Navigate to [Microsoft Copilot Studio](https://copilotstudio.microsoft.com)
2. Sign in with your M365 credentials
3. Click the **ellipsis (...)** in the left menu
4. Select **Solutions**

   ![Solutions Menu](images/is01a.png)

5. Click **Import solution** at the top

6. Click **Browse**
7. Select the Clara solution file: Download from [GitHub](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/agent/ClaraCopilotAgent_2_0_0_3.zip)
8. Click **Open**

   ![Import Solution](images/import01.png)

9. Click **Next**

   ![Next](images/import02.png)

10. Review the details and click **Next** again

    ![Review](images/import03.png)

11. Click **Import**

    ![Import](images/import04.png)

12. Wait for the import to complete (3-5 minutes)

    ![Importing](images/import05.png)

13. You may see a warning - this is expected and can be ignored

    ![Import Complete](images/import06.png)

### Task 4.2: Configure Clara Custom Connector

1. Open a new tab and navigate to [Power Automate](https://make.powerautomate.com)
2. In the left menu, find **Custom connectors** (you may need to click **More** > **Discover all**)

   ![Custom Connectors](images/import15a.png)

3. Locate **Clara API** connector
4. Click the **Edit** (pencil) icon

   ![Edit Connector](images/import14.png)

5. Go to the **Security** tab
6. Click **Edit** under Authentication

   ![Security Tab](images/import15.png)

7. Configure authentication:
   - **Identity Provider**: Generic OAuth 2
   - **Client Secret**: Enter the **Copilot Studio Client Secret** you saved earlier
8. Click **Update connector**

   ![Update Connector](images/import16.png)

### Task 4.3: Test Custom Connector

1. Click the **Test** tab
2. Click **+ New connection**

   ![New Connection](images/import17.png)

3. Sign in with your M365 credentials when prompted

4. Under **Operations**, find **GetCopilotLicenseAvailability**
5. Click **Test operation**

   ![Test Operation](images/import18.png)

6. Verify you receive a JSON response with:
   - totalLicenses
   - assignedLicenses
   - availableLicenses

### Task 4.4: Configure Clara Agent

1. Return to [Copilot Studio](https://copilotstudio.microsoft.com)
2. Go to the home page
3. Click on the **Clara** agent

   ![Open Clara](images/import07.png)

4. Go to the **Settings** tab
5. Verify the custom connectors are listed

   ![Verify Connectors](images/import08.png)

### Task 4.5: Test License Query

1. In the **Test your agent** panel, enter:
   ```
   How many Copilot licenses do we have, and how many are still available?
   ```

2. Click **Connect** when prompted

   ![Connect Prompt](images/import09.png)

3. On the **Manage your connections** page, click **Connect** again

   ![Manage Connections](images/import10.png)

4. Sign in with your M365 credentials

   ![Sign In](images/import11.png)

5. If you receive an **AADSTS50011** error:
   - Go to [Azure Portal](https://portal.azure.com)
   - Navigate to **Azure Active Directory** > **App registrations**
   - Select **Clara Copilot Agent - Copilot Studio**
   - Go to **Authentication**
   - Add the redirect URI shown in the error message
   - Click **Save**

   ![Fix Redirect URI](images/import13.png)

6. Retry the connection in Copilot Studio

7. Click **Retry** in the test panel

   ![Retry](images/import19.png)

8. Verify you receive a response like:
   ```
   You have a total of 25 Copilot licenses. Out of these, 7 licenses are currently assigned, leaving 18 licenses available for assignment.
   ```

   ![Success Response](images/import20.png)

### Task 4.6: Configure SharePoint Connection

1. Go to the **Tools** tab in Clara
2. Find and click **Get Copilot Waitlist Users**

   ![SharePoint Tool](images/st01.png)

3. Configure the connection:
   - **Site Address**: Your SharePoint site URL
   - **List Name**: `M365 Copilot Waitlist`
4. Click **Save**

   ![Configure SharePoint](images/st02.png)

### Task 4.7: Test SharePoint Integration

1. In the **Test your agent** panel, enter:
   ```
   What's the current waitlist status?
   ```

2. Click **Connect** when prompted

   ![SharePoint Connect](images/st03.png)

3. In **Manage your connections**, click **Connect** for SharePoint

   ![Connect SharePoint](images/st04.png)

4. Sign in and click **Submit**

   ![Submit Connection](images/st05.png)

5. Verify the connection shows as **Connected**

   ![Connected](images/st06.png)

6. Click **Retry** in the test panel

   ![Retry SharePoint](images/st07.png)

7. You should see a response about the waitlist status (may be empty if no users are on the waitlist)

### Task 4.8: Configure Waitlist Update Flow

1. Go to the **Topics** tab
2. Click **Update the waitlist status to approved**

   ![Update Topic](images/ul01.png)

3. In the topic, click **View flow details**

   ![View Flow](images/ul02.png)

4. Switch to the **Designer** tab

   ![Designer Tab](images/ul03.png)

5. In the **Update item** action, configure:
   - **Site Address**: Your SharePoint site URL
   - **List Name**: `M365 Copilot Waitlist`
6. Click **Publish**

   ![Publish Flow](images/ul04.png)

### Knowledge Check 4

Verify your Clara agent:
- [ ] Agent is imported successfully
- [ ] Custom connector is configured and tested
- [ ] License query returns data
- [ ] SharePoint connection is working
- [ ] Waitlist flow is configured

---

## Exercise 5: Test the Complete Solution

In this final exercise, you will perform end-to-end testing of the Clara solution.

### Task 5.1: Add Test User to Waitlist

1. Navigate to your SharePoint site
2. Open the **M365 Copilot Waitlist** list
3. Click **+ New** to add an item
4. Fill in the details:
   - **UserEmail**: Enter a test email
   - **User Waiting License**: Select a user
   - **Status**: Requested
5. Click **Save**

### Task 5.2: Test Waitlist Query

1. In Copilot Studio, test with:
   ```
   Show me the users on the waitlist
   ```

2. Verify Clara returns the user you just added

### Task 5.3: Test License Information

1. Test with:
   ```
   How many licenses are available?
   ```

2. Verify Clara returns accurate license counts

### Task 5.4: Test Waitlist Approval (Optional)

1. Test with:
   ```
   Approve [user email] from the waitlist
   ```

2. Check the SharePoint list to verify the status changed to "Approved"

### Task 5.5: Publish Clara

1. In Copilot Studio, click **Publish**
2. Select **Publish**
3. Wait for publishing to complete
4. Configure channels (Teams, etc.) as needed

---

## Lab Summary

Congratulations! You have successfully:
- ✅ Created and configured Azure AD app registrations for secure API access
- ✅ Deployed the Clara REST API to Azure App Service
- ✅ Created and configured a SharePoint waitlist for license management
- ✅ Imported and configured the Clara agent in Copilot Studio
- ✅ Tested the complete end-to-end solution

### What You Learned

- How to implement OAuth 2.0 authentication in Azure AD
- How to deploy .NET applications to Azure App Service
- How to integrate SharePoint lists with Copilot Studio
- How to configure custom connectors in Power Automate
- How to build and test AI agents in Microsoft Copilot Studio

### Next Steps

Now that you have Clara deployed, you can:
1. **Customize the agent** - Modify topics and responses to match your organization's needs
2. **Add more integrations** - Connect to additional data sources
3. **Enable notifications** - Set up email or Teams notifications for license events
4. **Monitor usage** - Track how users interact with Clara
5. **Scale the solution** - Deploy to production and onboard your organization

### Resources

- [Clara GitHub Repository](https://github.com/luishdemetrio/clara-copilot-agent)
- [Microsoft Copilot Studio Documentation](https://learn.microsoft.com/microsoft-copilot-studio)
- [Microsoft Graph API Reference](https://learn.microsoft.com/graph/api/overview)
- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service)

### Cleanup (Optional)

If this is a test environment, you can clean up resources:
1. Delete the Azure App Service and Resource Group
2. Delete the Azure AD app registrations
3. Delete the SharePoint list
4. Delete the Clara solution from Copilot Studio

---

## Troubleshooting

### Common Issues and Solutions

**Issue: AADSTS50011 Error**
- **Solution**: Add the redirect URI to your app registration's Authentication settings

**Issue: API returns 401 Unauthorized**
- **Solution**: Verify admin consent is granted for API permissions

**Issue: SharePoint connection fails**
- **Solution**: Ensure you have permissions to the SharePoint site and list

**Issue: Deployment to App Service fails**
- **Solution**: Check the deployment logs in Deployment Center and verify environment variables

**Issue: Custom connector test fails**
- **Solution**: Verify the API is running and accessible via Swagger UI

---

**End of Lab**

Thank you for completing this hands-on lab! If you have questions or feedback, please visit the [GitHub Discussions](https://github.com/luishdemetrio/clara-copilot-agent/discussions).
