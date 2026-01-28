# Exercise 2: Configure Azure App Registration

**Estimated time:** 12 minutes

## Objective

Configure the Azure AD application with proper API permissions, admin consent, client secret, and custom scope to enable secure access to Microsoft Graph.

---

## What You'll Do

- Add Microsoft Graph API permissions
- Grant admin consent for permissions
- Generate and securely save client secret
- Expose API with custom OAuth scope

---

## Background

CLARA needs specific Microsoft Graph permissions to manage Copilot licenses:

- **Directory.Read.All:** Read user and group information
- **GroupMember.ReadWrite.All:** Add/remove users from license group
- **Reports.Read.All:** Access M365 Copilot usage reports

---

## Tasks

### 🧱 Step 1: Locate CLARA App Registration

1. Open a **new browser tab**

2. Navigate to: https://portal.azure.com

3. Sign in with Skillable credentials (if prompted)

4. Search for and click: **App Registrations**

  ![](images/ap02.png)


5. Click **All applications** tab

6. Search for: **CLARA**

7. Click on the CLARA application

   ![](images/az01a.png)
   
   
✅ **Validation:** CLARA app registration Overview page is visible.

---

### 🧱 Step 2: Configure API Permissions

#### Understanding Permission Types
Before we add permissions, let's understand the critical difference between the two types:

##### Delegated Permissions:

- Actions are performed on behalf of a signed-in user
- The app can only do what the user themselves could do
- User must be logged in and consent to the permissions
- More secure—combines app permissions + user permissions (least privilege)
- **This is what Clara uses**

##### Application Permissions:

- Actions are performed as the app itself, without a signed-in user
- The app has full access regardless of which user is logged in
- Typically used for background services and daemons
- More powerful but requires tighter security controls

##### Why Clara Uses Delegated Permissions:
Clara operates in a conversational context where an IT admin is actively logged in and interacting with her. When Clara assigns a license, she does it on behalf of that admin, using their identity and permissions. This ensures:

- **Accountability**: Every action is traced to a real person
- **Security**: Clara can't do more than the logged-in admin could do manually
- **Auditability**: Logs show which admin made each decision
- **Least privilege**: Clara inherits the admin's permissions, nothing more

If we used Application permissions instead, Clara would have unrestricted access to perform actions even when no one is logged in—creating unnecessary security risk and losing the audit trail of who approved each action.

Steps:

1. In the app registration, click **API permissions** (left menu)

2. Click **+ Add a permission**

3. Select **Microsoft Graph**

   ![](images/az03a.png)

4. Click **Delegated permissions**

   ![](images/az03b.png)

   > ⚠️ Important: Choose Delegated permissions, NOT Application permissions. Clara needs to act on behalf of signed-in admins, not as an autonomous service.

5. Search for and select these **3 permissions**:

   **Permission 1:**
   - Search: `Directory.Read.All`
   - Expand **Directory**
   - Check ☑️ **Directory.Read.All**

   ![](images/az04a1.png)
   
   **Permission 2:**
   - Search: `GroupMember.ReadWrite.All`
   - Expand **GroupMember**
   - Check ☑️ **GroupMember.ReadWrite.All**
   
   ![](images/az04a2.png)

   **Permission 3:**
   - Search: `Reports.Read.All`
   - Expand **Reports**
   - Check ☑️ **Reports.Read.All**

6. Click **Add permissions**

   ![](images/az04a3.png)


✅ **Validation:** All 3 permissions appear in the API permissions list.

---

### 🧱 Step 3: Grant Admin Consent

1. On the API permissions page, click **Grant admin consent for [Your Organization]**

   ![](images/az04a.png)

2. Click **Yes** in the confirmation dialog

3. Wait for consent to be granted (2-5 seconds)

4. Verify **Status** column shows **Granted** with green checkmarks

   ![](images/az04a4.png)

✅ **Validation:** All 3 permissions show green checkmarks with "Granted" status.

**Troubleshooting:**
- **Button grayed out:** You may need Global Admin role—notify your proctor
- **Consent fails:** Wait 30 seconds and try again

---

### 🧱 Step 4: Generate a Client Secret

#### Why Clara Needs a Client Secret
A client secret is like a password for the Azure App Registration. When Clara's custom connector authenticates with Microsoft Graph API, it needs to prove two things:

1. Who it is (using the Client ID from Step 1)
2. That it's authorized (using the Client Secret we're about to create)

Together, the Client ID and Client Secret form Clara's authentication credentials. Think of it as a username and password combination—except this "password" is a cryptographically secure string that enables OAuth 2.0 authentication flows between Clara, your tenant, and Microsoft Graph API.

The client secret is sensitive information. Anyone with both the Client ID and Client Secret could potentially authenticate as Clara and perform actions on behalf of users. That's why Azure shows it only once—to minimize exposure risk.

Steps:

1. Click **Certificates & secrets** (left menu)

2. Under **Client secrets** tab, click **+ New client secret**

3. Enter details:
   - **Description:** `Clara API Secret`
   - **Expires:** **6 months** (or an option of your choice)

4. Click **Add**

   ![](images/az08.png)

5. 🚨 **IMMEDIATELY copy the VALUE** (long string under "Value" column)

   ![](images/az09.png)

   > ⚠️ Do NOT copy the "Secret ID"—copy the Value column. The Secret ID is just a reference identifier; the Value is the actual credential.

6. **Paste into Notepad** immediately:
   ```
   Client Secret Value: ________________________
   ```

> 🚨 CRITICAL: The secret value is shown only ONCE. After you navigate away from this page, it cannot be retrieved. If you lose it, you'll need to delete this secret and create a new one.


7. While you're here, verify you have the Client ID and Tenant ID from the previous exercise. If you missed copying them, go to the Overview page and copy:
   - **Application (client) ID**
   - **Directory (tenant) ID**

8. Update your complete configuration tracker:
   ```
   Application (client) ID: ____________________
   Directory (tenant) ID: ______________________
   Client Secret Value: ________________________
   ```

✅ **Validation:** All 3 values safely saved in Notepad.

**Troubleshooting:**

- **Lost the secret?** You cannot retrieve it. Delete the old secret (⋯ → Delete) and create a new one following steps 2-6 above.
- **Can't create secret?** Verify you have the necessary permissions on the app registration—notify your proctor if the issue persists.
- **Copied the wrong value?** If you accidentally copied the Secret ID instead of the Value, delete the secret and create a new one.

---

### 🧱 Step 5: Expose the API

#### Why We Need to Expose an API
Up to this point, we've configured Clara to call Microsoft Graph API (the permissions in Step 2). Now we need to do the reverse: allow Clara's Custom Connector to securely call Clara's app registration using OAuth 2.0 authentication.

##### Understanding the Flow:
When a user interacts with Clara, here's what happens behind the scenes:

1. User logs in to Clara through Copilot Studio
2. Custom Connector authenticates with Clara's app registration (this step)
3. Clara's app uses delegated permissions to call Microsoft Graph API
4. Actions are performed on behalf of the logged-in user

##### What "Expose an API" Does:

By exposing an API, you're essentially saying: "This app registration can be called by other applications, and here are the rules for who can access it and what they can do."

###### The Application ID URI:

Think of this as Clara's unique API address. Just like a website has a URL (https://www.example.com), your app registration needs a URI that identifies it in your tenant. Azure uses the format api://[client-id] to ensure uniqueness.

###### OAuth 2.0 Scopes:

Scopes define what level of access is being requested. The access_as_user scope we're creating tells Azure: "When someone authenticates through this app, they're acting as themselves, not as the app." This ties directly to the delegated permissions model from Step 2.

Steps:

1. Click **Expose an API** (left menu)

2. Next to **Application ID URI**, click **Add**

3. Azure suggests: `api://[your-application-id]`

   Keep this default value—do NOT change it
   
   > 💡 Why? This format ensures your API URI is globally unique within Azure AD. Custom URIs can cause authentication conflicts.

4. Click **Save**

   ![](images/az06.png)

5. Under **Scopes defined by this API**, click **+ Add a scope**

6. Fill in the scope details:

   **Scope name:** `access_as_user`
   
   > 💡 This is a conventional name that clearly indicates delegated access

   **Who can consent:** 
   - Select **Admins and users**
   
   > 💡 Allows both admins and end users to consent, providing flexibility while maintaining security

   **Admin consent display name:** `Access Clara as user`

   **Admin consent description:** `Allows Clara access on behalf of the user`

   **User consent display name:** `Access Clara as user`

   **User consent description:** `Allows Clara to act on your behalf`

   **State:**
   - Ensure **Enabled** is selected

7. Click **Add scope**

   ![](images/az07.png)
   
8. Verify the scope appears in the format:
   ```
   api://[your-client-id]/access_as_user
   ```

9. **Copy the full scope URI** to Notepad:
   ```
   Scope URI: api://[client-id]/access_as_user
   ```

   ![](images/az07a.png)
   
   > 🚨 Important: Copy the COMPLETE URI including api:// prefix. You'll need this exact format in Exercise 3.
   
10. Update your configuration tracker with the new value:
    
    ```
    Application (client) ID: ____________________
    Directory (tenant) ID: ______________________
    Client Secret Value: ________________________
    Scope URI: api://[client-id]/access_as_user
    ```
   
✅ **Validation:** Scope visible with Status "Enabled" and full URI saved in Notepad.

---

## Summary

You've configured:

- ✅ 3 Microsoft Graph delegated permissions
- ✅ Admin consent granted for all permissions
- ✅ Client secret created and saved
- ✅ API exposed with custom `access_as_user` scope

---

## Troubleshooting

**Issue:** Can't add delegated permissions

**Solutions:**
- **Can't add scope?** Ensure you saved the Application ID URI in step 4 first
- **Scope already exists?** Someone may have created it previously—verify the configuration matches and proceed
- **Wrong Application ID URI?** You can edit it, but only before adding scopes. If scopes exist, delete them first, change the URI, then recreate scopes.

---

**Issue:** Lost client secret

**Solutions:**
- You CANNOT retrieve it
- Go to Certificates & secrets
- Delete old secret (⋯ → Delete)
- Create new secret
- Update your notes

---

**Issue:** Scope already exists

**Solutions:**
- Click existing scope to verify configuration
- If correct, proceed to Exercise 3
- If wrong, delete and recreate

---

**Previous:** [Exercise 1: Import CLARA](./01-exercise1.md)  
**Next:** [Exercise 3: Configure Clara Custom Connector](./03-exercise3.md)
