---
name: "waitlist-management-servicenow"
description: "Manages the M365 Copilot license waitlist stored in ServiceNow. Covers reviewing pending license requests submitted through the Request Microsoft 365 Copilot License catalog item and approving users."
---

# Waitlist Management Skill (ServiceNow)

Use this skill to manage the M365 Copilot license waitlist stored in ServiceNow. This skill covers reviewing pending license requests submitted through the "Request Microsoft 365 Copilot License" catalog item and approving users.

## When this skill is activated

The user is asking about who is waiting for a Copilot license, wants to review the ServiceNow request queue, or wants to approve a specific user's RITM.

1. Call **ServiceNow - List Records** to retrieve pending requests from the `u_copilot_license_request` table (filtered to `u_status = Requested`). The response includes: `sys_id`, `u_display_name`, `u_email`, and `u_business_justification` for each record.
2. Present the results to the user as a clear, readable table or list — include at minimum: display name, email, and business justification for each pending request.
3. If the user asks to approve someone, confirm the target user with the admin before proceeding.
4. Once confirmed, call **ServiceNow - Update Record** using the record's `sys_id`, setting `u_status` to `Assigned`, `u_assignment_date` to today's date, and optionally recording `u_notes` if the admin provides a reason.
5. Confirm the outcome back to the user — whether the update succeeded or failed.

## Guidelines

- Always retrieve the current waitlist before attempting any approval — never assume the list contents from a previous turn, as new requests may have arrived.
- If **ServiceNow - List Records** returns no results, tell the user there are no pending Copilot license requests in ServiceNow. Do not attempt an update.
- The `sys_id` for **ServiceNow - Update Record** must come from the `sys_id` field returned by **ServiceNow - List Records** for that specific record — never guess or construct a sys_id.
- Before approving, always confirm the exact user with the admin — particularly when the request uses a partial name or an ambiguous match (e.g., two users named "John").
- Do not approve multiple users in a single action without explicit confirmation for each — ask the admin to confirm each approval individually.
- If the admin provides a reason or comment for the approval (e.g., "assigned — high priority project"), include it in the `u_notes` field of the update call.
- If ServiceNow cannot be reached or returns an error, surface the error clearly rather than silently failing.
- This skill only handles **approval** and updates the waitlist record status. Actual Copilot license assignment is handled by the Individual License Provisioning or Entra ID Group License Provisioning skill — do not attempt to assign a license directly from this skill.

## Examples

**Example 1: Reviewing the waitlist**
- User request: `"Show me the pending Copilot license requests"` / `"Who is waiting for a Copilot license in ServiceNow?"` / `"Show the waitlist"`
- Expected behavior: Call **ServiceNow - List Records**, then display all pending entries with display name, email, and business justification. If the list is empty, say so.

**Example 2: Approving a specific user**
- User request: `"Approve John from the waitlist"` / `"John Doe is assigned"` / `"Approve or assign to the first person on the list"`
- Expected behavior: If the waitlist hasn't been retrieved yet in this conversation, call **ServiceNow - List Records** first. Identify the matching record, note its `sys_id`, and confirm with the admin ("I'll approve John Doe (johndoe@contoso.com) — shall I proceed?"). On confirmation, call **ServiceNow - Update Record** with the `sys_id`, `u_status = Approved`, and today's date. Report success or failure.

**Example 3: Approving with a note**
- User request: `"Approve Jane, she needs it for the Q3 project"`
- Expected behavior: Retrieve the waitlist if needed, identify Jane's record, confirm with the admin, then call **ServiceNow - Update Record** with `u_status = Assigned`, today's date, and `u_notes = "Assigned for Q3 project"`. Report the result.

**Example 4: Ambiguous name**
- User request: `"Approve John"`
- Expected behavior: If multiple records with the display name "John" appear in the results, list them (with email and business justification) and ask the admin to clarify which one before proceeding.

**Example 5: Empty queue**
- User request: `"Show me the waitlist"`
- Expected behavior: Call **ServiceNow - List Records**. If no results are returned, respond: "There are no pending Copilot license requests in ServiceNow right now."

## Notes

- The ServiceNow table used by this skill is `u_copilot_license_request`, which corresponds to the "Request Microsoft 365 Copilot License" catalog item. Requests are filtered by `u_status = Requested` — only unprocessed requests appear.
- Fields returned per record: `sys_id` (required for updates), `u_display_name`, `u_email`, `u_business_justification`. Additional fields available in the schema include `u_status`, `u_assignment_date`, `u_license_assigned`, and `u_notes`.
- The `sys_id` is the ServiceNow internal record identifier and is not visible to end users — it is used only internally by **ServiceNow - Update Record**.
- This skill does not handle rejection — if the admin wants to reject a request, they should update the record directly in ServiceNow, or note this as a future capability.
- After an approval, the admin will likely want to proceed to license assignment — proactively ask if they'd like to assign the Copilot license to the approved user now.
