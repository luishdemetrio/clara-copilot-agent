---
name: "waitlist-management-sharepoint"
description: "Manages the M365 Copilot license waitlist stored in a SharePoint list. Covers reviewing pending requests, approving users, and initiating license assignment."
---

# Waitlist Management Skill (SharePoint List)

Use this skill to manage the M365 Copilot license waitlist stored in a SharePoint list. This skill covers retrieving pending requests, approving users, and handing off to license assignment.

## When this skill is activated

The user is asking about who is waiting for a Copilot license, wants to review the waitlist queue, wants to approve a specific user, or wants to assign a license to someone from the waitlist.

## 🛠 Action 1: Retrieve Waitlist Users

1. Call **Get Copilot Waitlist Users** to retrieve pending requests from the configured SharePoint view.
2. From the response, extract for each entry: **ID**, name, email, status, and request date.
3. Present the results in a clear, sortable table — always include the SharePoint **ID** column as the first field so the admin can reference it unambiguously.
4. Internally store the mapping of SharePoint ID → user for use in any subsequent approval or update action.

> ⚠️ **Critical:** Never use a row's list position as its SharePoint item ID. Always map the position to the **ID column value** before calling any update. Example: if the 2nd item in the list has ID 4, use ID 4 — not 2.

## 🛠 Action 2: Approve a User and Initiate License Assignment

> ⚠️ **Critical:** Requires explicit admin confirmation at each step. Follow the steps below in order — do not skip or combine them.

### Step 1 — Identify the target user

- Ask for or confirm the target user by name or email.
- If the user came from the waitlist retrieved in Action 1, look up their **SharePoint ID column value** from the stored mapping — never use their row position.
- If the user was not from the waitlist (direct assignment request), proceed to license assignment without a waitlist update.

### Step 2 — Update waitlist status (if applicable)

- If the user came from the waitlist, call **Manage SharePoint Waitlist** to set their status to **Approved**, passing the **SharePoint ID column value** (not the row number).
- Confirm the update succeeded before proceeding.

> ⚠️ **Critical:** The SharePoint item ID used here must be the **ID column value** returned by **Get Copilot Waitlist Users** — never infer or guess it from list position.

### Step 3 — Hand off to license assignment

- After a successful waitlist update (or immediately, for non-waitlist requests), proactively ask: "Would you like me to assign the Copilot license to [user] now?"
- If confirmed, pass the user's email to the Individual License Provisioning or Entra ID Group License Provisioning skill to complete the assignment.

## Guidelines

- Always retrieve the current waitlist before attempting any approval — never assume list contents from a previous turn.
- If **Get Copilot Waitlist Users** returns no results, tell the admin the waitlist is empty. Do not attempt an update.
- Before approving, always confirm the exact user — especially when the request uses a partial name or an ambiguous match (e.g., two users named "John").
- Do not approve multiple users in a single action without explicit confirmation for each one individually.
- If the SharePoint list or view cannot be reached, surface the error clearly rather than silently failing.
- The status updated in SharePoint tracks the waitlist record only — it does not assign a license. License assignment is always a separate, subsequent step.

## Examples

**Example 1: Reviewing the waitlist**
- User request: `"Show me the pending waitlist requests"` / `"Who is waiting for a Copilot license?"` / `"Show the waitlist"`
- Expected behavior: Call **Get Copilot Waitlist Users**, display all pending entries as a table with ID, name, email, status, and request date. If empty, say so.

**Example 2: Approving a user from the waitlist**
- User request: `"Approve John from the waitlist"` / `"John Doe is approved"` / `"Approve the first person on the list"`
- Expected behavior: If the waitlist hasn't been retrieved yet, call **Get Copilot Waitlist Users** first. Identify the matching user and their SharePoint ID value. Confirm with the admin ("I'll approve John Doe (johndoe@contoso.com), SharePoint ID 4 — shall I proceed?"). On confirmation, call **Manage SharePoint Waitlist** using ID 4. Report success, then ask if the admin wants to proceed with license assignment.

**Example 3: Assigning a license directly (not from waitlist)**
- User request: `"Assign a Copilot license to jane@contoso.com"`
- Expected behavior: No waitlist update needed. Confirm the target user with the admin, then hand off directly to the license assignment skill.

**Example 4: Ambiguous name**
- User request: `"Approve John"`
- Expected behavior: If multiple users named John appear in the waitlist, list them (with ID, email, and request date) and ask the admin to clarify which one before proceeding.

**Example 5: ID vs. position confusion**
- User request: `"Approve the second person on the list"`
- Expected behavior: Retrieve the waitlist, identify the 2nd displayed entry, read its **ID column value** (e.g., ID 7), and use ID 7 for the update — not position 2. Confirm with the admin before proceeding.

**Example 6: Empty waitlist**
- User request: `"Show me the waitlist"`
- Expected behavior: Call **Get Copilot Waitlist Users**. If no results are returned, respond: "There are no pending Copilot license requests in the waitlist right now."

## Notes

- The SharePoint list and view are configured through environment variables (`SharePoint Waitlist Site URL`, `SharePoint Waitlist List Name`, `SharePoint Waitlist View ID`). If the skill consistently returns no results or connection errors, ask the administrator to verify these values in the solution settings.
- The View ID used by **Get Copilot Waitlist Users** is a GUID, not the view's display name — a common misconfiguration source.
- This skill does not handle rejection. If the admin wants to reject a request, direct them to update the SharePoint list directly, or flag it as a future capability.
- The SharePoint **ID** field is an auto-incremented integer assigned by SharePoint — it is stable and unique within the list, unlike row position which changes as items are filtered, sorted, or deleted.
