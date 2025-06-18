# 👧 CLARA -Copilot License Assignment & Report Agent

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-Microsoft%20Copilot%20Studio-blue)](https://copilotstudio.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-REST%20API-purple)](https://dotnet.microsoft.com/)

**Clara** is an intelligent AI agent built on Microsoft Copilot Studio that revolutionizes M365 Copilot license management for enterprises. It automates license monitoring, optimizes allocation, and streamlines user communication to ensure maximum ROI on your M365 Copilot investment.

| [Documentation](https://github.com/luishdemetrio/clara-copilot-agent) |  [1. Azure Application Registration guide ](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/azure_deployment.md)  | [2. Azure REST API](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/appservice_deployment.md) |[3. SharePoint M365 Copilot Wait List](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/sharepoint_deployment.md) |[4. Import CLARA to Copilot Studio](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/import_clara.md) | [(Opcional) Local Deployment guide ](https://github.com/luishdemetrio/clara-copilot-agent/blob/main/docs/local_deployment.md)
| ---- | ---- | ---- |  ---- | ---- | ---- |  


## Create the Required SharePoint Lists

Before beginning the Clara agent configuration,  you must create SharePoint lists to track users waiting for M365 Copilot licenses.

### 🧱  Step 1: Navigate to SharePoint

1. Open your web browser
2. Navigate to your SharePoint site where you want to create the lists
   - Example: `https://yourcompany.sharepoint.com/sites/YourSiteName`
3. Sign in with your Microsoft 365 credentials if prompted

---
### 🧱  Step 2: Create the M365 Copilot Waitlist

1. From your SharePoint site homepage, click **"New"** in the top navigation
2. Select **"List"** from the dropdown menu 

   ![](images/sl01.png)
   
3. Choose **"List"** option

   ![](images/sl02.png)
   
4. Configure the list:
   - **Name**: `M365 Copilot Waitlist`
   
5. Click **"Create"**

   ![](images/sl03.png)

---
### 🧱  Step 3: Add Required Columns

1. Hide the Default Title Column

 - Since the Title column is not needed, hide it for a cleaner list view.

   ![](images/sl07.png)

2. Add Columns

  **a. UserEmail**
  
    - Click **"+ Add column"**

      ![](images/sl04.png)
  
    - Select **"Text"** and then **Next**:
 
      ![](images/sl05.png)
  
    - **Column name**: `UserEmail`
    - **Description**: `User email`
    - **Type**: Select **"Single line of text"**
    - Click **"Save"**

      ![](images/sl06.png)

  **b. User Waiting License**
  
    - Click **"+ Add column"**
    - Select **"Person"** and then **Next**:

      ![](images/sl08.png)
  
    - **Column name**: `User Waiting License`
    - **Description**: `User Waiting License`
    - **Type**: Select **"Person or Group"**
    - Click **"Save"**

      ![](images/sl09.png)

 **c. Status**
 
    - Click **"+ Add column"**
      ![](images/sl10.png)

    - Select **"Choice"** and then **Next**:
      ![](images/sl11.png)
  
    - **Column name**: `Status`
    - **Description**: `Status`
    - **Type**: Select **"Choice"**
    - Add the choices:
      - Requested
      - Approved
    - Click **"Save"**
      ![](images/sl12.png)
  
 **d. Approved By**
 
    - Click **"+ Add column"**
    - Select **"Person or Group"** and then **Next**:
      ![](images/sl13.png)
  
    - **Column name**: `Approved By`
    - **Description**: `Person who approved the M365 Copilot License`
    - **Allow multiple selections**: Leave unchecked
    - Click **"Save"**

      ![](images/sl14.png)

**Expected Result:**

Your list should now look like this:

![](images/sl15.png)