# 👧 CLARA -Copilot License Assignment & Report Agent

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-Microsoft%20Copilot%20Studio-blue)](https://copilotstudio.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-REST%20API-purple)](https://dotnet.microsoft.com/)

**Clara** is an intelligent AI agent built on Microsoft Copilot Studio that revolutionizes M365 Copilot license management for enterprises. It automates license monitoring, optimizes allocation, and streamlines user communication to ensure maximum ROI on your M365 Copilot investment.

![](images/Clara.png)

| [Documentation](https://github.com/luishdemetrio/clara-copilot-agent) |  [Azure Application Registration guide ](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/azure_deployment.md)  | [Local Deployment guide](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/local_deployment.md) |[SharePoint M365 Copilot Wait List](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/sharepoint_deployment.md) |[Import CLARA to Copilot Studio](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/import_clara.md)
| ---- | ---- | ---- |  ---- | ---- |  


## Importing CLARA Agent into Copilot Studio


---
### 🧱  Step 1: Access Microsoft Copilot Studio

**1. Navigate to Copilot Studio**
   - Open your web browser
   - Go to [Microsoft Copilot Studio](https://copilotstudio.microsoft.com)
   - Sign in with your Microsoft 365 credentials

**2. Access Solutions**

   - On the left-hand side menu, click on the **ellipsis**  (...) and then select **Solutions**.
   
   ![](images/is01a.png)

---
### 🧱  Step 2: Import the Agent

**1. Start Import**

   - In the top menu, click **Import solution**.
   
   - Click **Browse**, locate [Clara](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/agent/ClaraCopilotAgent_1_0_0_1.zip) solution file, select it, and click Open  

     ![](images/import01.png)
 
**2. Proceed with Import**
 
   - Click Next to continue.
   
     ![](images/import02.png) 
       
   - Review the details, then click Next again.  
   
     ![](images/import03.png) 
        
   - Click Import to begin.
   
       ![](images/import04.png) 
       
**3. Wait for Completion**       

   - The import may take a few minutes.
   
     ![](images/import05.png) 
       
**4. Import Complete**              
   - You may see a warning after import—this is expected and relates to SharePoint configuration. You can ignore it for now.
   
   - The agent will appear in your solutions list .
   
     ![](images/import06.png) 
  
---
### 🧱  Step 3: Configure the Clara Custom Connector in Power Automate

**1. Access Power Automate**

   - Open your web browser and go to [Power Automate](https://make.powerautomate.com/).
   - Sign in with your Microsoft 365 credentials.

**2. Locate Custom Connectors**
   
   - In the left-hand menu, look for Custom Connectors.
  
     ![](images/import15a.png) 
   
   - If **Custom Connectors** is not pinned:
   
     - Click on **More** at the bottom of the menu.
     - Select **Discover all**.
     - Find and click on **Custom Connectors** in the expanded list.

**3. Find and Edit the Clara Connector**

   - In the Custom Connectors list, locate the **Clara API** connector.
   
   - Click the **Edit** (pencil) icon next to the **Clara API** connector.
   
     ![](images/import14.png) 
     
**4. Update Authentication Settings**

   - Go to the **Security** tab.
   
   - Under **Authentication Type**, click **Edit**.
   
       ![](images/import15.png) 
       
**5. Change Identity Provider**

   - Set the Identity Provider to **Generic OAuth 2**.
   
   - Enter the **Client Secret** for the **Clara Copilot Agent - Copilot Studio** App Registration in the appropriate field. 
   
     > Please check the [Azure Application Registration guide ](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/azure_deployment.md) for more details.
   
   - Click **Update connector** to save your changes.
   
     ![](images/import16.png) 

**6. Go to the Test Tab**

   - Click on the Test tab at the top.

**7. Create a New Connection**

   - Click **+ New connection**.
   
   - Sign in using your Microsoft 365 credentials when prompted.
   
     ![](images/import17.png) 
   
**8. Test the Operation**

   - In the list of available operations, find **GetCopilotLicenseAvailability**.
   
   - Click on **Test operation**.
   
     ![](images/import18.png) 
     
**9. Review the Results**

   - You should receive a JSON response containing:
     - totalLicenses
     - assignedLicenses
     - availableLicenses
     
   - Example response:
   
      ```json
         {
           "totalLicenses": 100,
           "assignedLicenses": 75,
           "availableLicenses": 25
         }
      ```


---
### 🧱  Step 4: Open the Agent in Copilot Studio


**1. Return to Copilot Studio Home**

   - After importing, go back to the Copilot Studio home page.
   - Your agent (Clara) should now be visible in the list.
   - Click on the Clara agent to open it.
   
     ![](images/import07.png)

  

**3. Verify Custom Connectors**

   - Go to the Tools tab.
   - Confirm that the custom connectors are listed.
     
     ![](images/import08.png) 

**4. Test the Copilot License Availability**     

   - In the Test your agent panel, use the following prompt:

     `How many Copilot licenses do we have, and how many are still available?`
     
   - You will be prompted to connect. Click Connect.

     ![](images/import09.png) 

**5. Manage Your Connections**

   - On the Manage your connections page, click Connect again.
   
     ![](images/import10.png) 
     
   - Sign in with your Microsoft 365 credentials.     
     
     ![](images/import11.png) 
     

**6. Troubleshooting: AADSTS50011 Error**

   - If you receive the error message AADSTS50011: The reply URL specified in the request does not match the reply URLs configured for the application, you need to update your app registration:
    
     ![](images/import12.png)
      
     1. Access the [Azure Portal](https://portal.azure.com/).
     2. Navigate to Azure Active Directory > App registrations.
     3. Find and select the **Clara Copilot Agent - Copilot Studio** app registration.
     4. Go to the **Authentication** menu.
     5. In the **Web - Redirect URIs** section, add the redirect URI shown in Copilot Studio.
     6. Click **Save**.     
     
    
        ![](images/import13.png) 

**7. Retry the Connection**

   - Return to Copilot Studio and try signing in again.
   
   - After successful sign-in, go back to the Test your agent panel and click Retry.     
   
     ![](images/import19.png)   
     
**8. Review the Agent’s Response**

   - You should receive an answer similar to:

     ```plaintext
     You have a total of 25 Copilot licenses. Out of these, 7 licenses are currently assigned, leaving 18 licenses available for assignment.     
     ```
     
     ![](images/import20.png) 