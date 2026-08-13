---
name: "license-management-individual"
description: "Manages M365 Copilot license assignment, reassignment, and bulk reassignment for individual users via the Graph API. Covers license availability checks, already-licensed user handling, waitlist status updates, and welcome email delivery."
---

## Context

- This skill handles direct, per-user license operations via the Microsoft Graph API (`assignLicense` endpoint).
- It covers three actions: **assigning** a license to a new user, **reassigning** a license from one user to another, and **bulk reassigning** available licenses to waitlisted users.
- All actions require **explicit operator confirmation** before any write operation is executed. Never proceed without it.
- If the waitlist integration (SharePoint List) is active, the stored SharePoint **ID column value** (not row position) must be used for any waitlist status update.
- If the welcome email capability is available, it should be offered after every successful assignment.

---

## 🛠 Action 4: Assign a License

> ⚠️ **Critical:** Requires explicit confirmation. Follow steps in order. Do not skip or combine steps.

### Step 1 — Check license availability

Call **Clara Graph APIs - M365 Copilot License Overview** to get current license counts.

- **No licenses available →** Inform the operator. Suggest alternatives:
  - Review the waitlist and identify users who may no longer need a license
  - Add the target user to the waitlist
  - Contact an administrator to purchase additional licenses
  - Send a welcome email if the user will be licensed through another process
  - **Stop here. Do not proceed.**
- **Licenses available →** Proceed to Step 2.

### Step 2 — Identify the target user

Ask for or confirm the target user (name or email).

- **From waitlist (SharePoint List):** retrieve and store the **SharePoint ID column value** for this user (not their row position in the list). This will be needed for Step 4.
- **Not from waitlist:** proceed normally.

### Step 3 — Present summary and request confirmation

Present a summary to the operator and ask for explicit confirmation before proceeding:

```
📋 License Assignment Summary

• Target user:        [Full name] ([email])
• Licenses available: [N]
• Waitlist origin:    Yes (SharePoint ID: [N]) / No
• Actions to perform:
    - Check if user already has a license
    - Assign M365 Copilot license (if not already licensed)
    - Update waitlist status to Approved (if from waitlist)
    - Send welcome email (if available)

Do you confirm? (Yes / Cancel)
```

Do **not** proceed without explicit confirmation. If the operator selects Cancel, terminate with no further changes.

### Step 4 — Execute (only after confirmation received)

Execute each sub-step in order. Do not begin the next sub-step until the current one returns successfully.

#### 4.1 — Check existing license status

Call **Clara Graph APIs - Check User License Individual** to verify whether the user already has a Copilot license assigned.

#### 4.2 — Handle already-licensed users

**If the user is already licensed →** Do not proceed with assignment. Inform the operator and present the following options:

```
⚠️ [User] already has a Copilot license assigned.

What would you like to do?
  A) Update waitlist status to Approved (if from waitlist)
  B) Send Welcome Email
  C) Cancel
```

Wait for the operator's selection. Execute **only** the selected option. After executing the selected step, **stop** — do not continue to any further steps.

**If the user is not yet licensed →** Proceed to 4.3.

#### 4.3 — Assign the license

Call **Clara - Add License Individual** to assign the M365 Copilot license to the target user.

- If the assignment fails, surface the error clearly and stop. Do not attempt the welcome email.
- If the assignment succeeds, proceed to 4.4.

#### 4.4 — Update waitlist status (if applicable)

If the user came from the waitlist → call **Manage SharePoint Waitlist** to set their status to **Approved**, using the **SharePoint ID column value** stored in Step 2.

> ⚠️ Use the ID column value, not the row position.

#### 4.5 — Send welcome email (if available)

If the welcome email capability is available, send it now without waiting for additional input. Call **Send Welcome Email to Copilot Licensed User**.

Confirm the full outcome to the operator:

```
✅ Done

• License assigned to: [Name] ([email])
• Waitlist status updated: Yes / No / N/A
• Welcome email sent: Yes / Not available
```

---

## 🛠 Action 5: License Reassignment (Remove + Assign)

Moves a license from one user (source) to another (target).

> ⚠️ **Critical:** Requires explicit confirmation before any change is made.

### Step 1 — Identify both users

Ask for or confirm:
- **Source user** — the user whose license will be removed
- **Target user** — the user who will receive the license

If the target user comes from the waitlist, retrieve and store their **SharePoint ID column value**.

### Step 2 — Present summary and request confirmation

```
📋 License Reassignment Summary

• Remove license from:  [Source name] ([source email])
• Assign license to:    [Target name] ([target email])
• Waitlist origin:      Yes (SharePoint ID: [N]) / No
• Actions to perform:
    - Remove Copilot license from [source]
    - Assign Copilot license to [target]
    - Update waitlist status to Approved (if target from waitlist)
    - Send welcome email to [target] (if available)

Do you confirm? (Yes / Cancel)
```

Do **not** proceed without explicit confirmation. If the operator selects Cancel, terminate with no further changes.

### Step 3 — Execute in order

Execute each call in sequence. Do not proceed to the next call until the current one returns successfully.

**3.1 — Remove license from source user**

Call **Clara - Remove License Individual** for the source user.

**3.2 — Assign license to target user**

Call **Clara - Add License Individual** for the target user.

**3.3 — Update waitlist status (if applicable)**

If the target user came from the waitlist → call **Manage SharePoint Waitlist** to set their status to **Approved**, using the stored **SharePoint ID column value**.

**3.4 — Send welcome email (if available)**

Call **Send Welcome Email to Copilot Licensed User** for the target user. Do not wait for additional input.

**3.5 — Confirm outcome**

```
✅ Done

• License removed from: [Source name] ([source email])
• License assigned to:  [Target name] ([target email])
• Waitlist status updated: Yes / No / N/A
• Welcome email sent: Yes / Not available
```

> 📝 **Notes field:** When updating the waitlist status in step 3.3, populate `statusChangeNotes` with a brief rationale. Include:
> - That the license was assigned via CLARA
> - Whether the user was already in the relevant Entra ID group or was added during this operation

---

## 🛠 Action 6: Bulk Reassign to Waitlist

Reassigns all currently available licenses to the highest-priority users on the waitlist.

> ⚠️ **Critical:** Requires explicit confirmation before any changes are made. This action affects multiple users — review the full summary carefully before confirming.

### Step 1 — Identify available licenses and waitlist candidates

1. Call **Clara Graph APIs - M365 Copilot License Overview** to get the number of available licenses.
2. Call **Get Copilot Waitlist Users** to retrieve pending waitlist entries (sorted by request date ascending — oldest request first = highest priority).
3. Match: take the first N waitlist users, where N = number of available licenses.

If no licenses are available, inform the operator and stop.
If the waitlist has fewer entries than available licenses, inform the operator of the surplus.

### Step 2 — Present summary and request confirmation

```
📋 Bulk License Reassignment Summary

Available licenses: [N]
Waitlist users to assign ([N] total):

  #  | SP ID | Name              | Email                     | Requested
  -- | ----- | ----------------- | ------------------------- | ----------
  1  | [ID]  | [Name]            | [email]                   | [date]
  2  | [ID]  | [Name]            | [email]                   | [date]
  ...

Actions to perform for each user:
  - Check if already licensed
  - Assign M365 Copilot license
  - Update SharePoint waitlist status to Approved
  - Send welcome email (if available)

Do you confirm? (Yes / Cancel)
```

Do **not** proceed without explicit confirmation. If the operator selects Cancel, terminate with no further changes.

### Step 3 — Execute for each user in order

For each user in the list, execute the following sequence before moving to the next user:

**3.1 — Check existing license**

Call **Clara Graph APIs - Check User License Individual**.

- If already licensed: skip license assignment, update waitlist status to Approved, send welcome email. Log that this user was already licensed and move to the next.
- If not licensed: proceed to 3.2.

**3.2 — Assign license**

Call **Clara - Add License Individual**.

- If assignment fails: log the failure, skip the waitlist update and welcome email for this user, and continue to the next user. Do not stop the entire bulk operation for a single failure.

**3.3 — Update waitlist status**

Call **Manage SharePoint Waitlist** to set the user's status to **Approved**, using their **SharePoint ID column value** from the list retrieved in Step 1.

> ⚠️ Use the ID column value, not the row position.

**3.4 — Send welcome email (if available)**

Call **Send Welcome Email to Copilot Licensed User**. Do not wait for additional input before moving to the next user.

### Step 4 — Present final bulk summary

After processing all users, present a complete outcome report:

```
✅ Bulk Reassignment Complete

  Name              | Email               | Licensed | Waitlist Updated | Email Sent | Notes
  ----------------- | ------------------- | -------- | ---------------- | ---------- | ------
  [Name]            | [email]             | ✅ Yes   | ✅ Yes           | ✅ Yes     |
  [Name]            | [email]             | ⚠️ Skip  | ✅ Yes           | ✅ Yes     | Already licensed
  [Name]            | [email]             | ❌ Failed| ❌ Skipped       | ❌ Skipped | [error]

Total assigned: [N] / [N attempted]
```

---

## Guidelines

- **Confirmation is non-negotiable.** Never execute any write operation (assign, remove, waitlist update, email) without explicit operator confirmation in the current turn.
- **Step sequencing is mandatory.** Each step must complete successfully before the next begins — do not parallelize or skip steps.
- **SharePoint ID vs. row position.** When updating waitlist status, always use the **ID column value** from SharePoint, not the display order position. Store this at the time of retrieval and pass it explicitly to every update call.
- **Already-licensed users.** Detecting an already-licensed user is not an error — it's a checkpoint. Present the available options and wait for the operator's explicit choice before doing anything.
- **Bulk failures are non-fatal.** In Action 6, a single assignment failure should not abort the remaining users. Log the failure, skip that user's downstream steps, and continue.
- **Notes field in waitlist updates.** When updating the waitlist status as part of a reassignment, populate `statusChangeNotes` with whether the user was already in an Entra ID group or was newly added, and that the action was performed via CLARA.

## Examples

**Example 1: Direct assignment, no waitlist**
- User request: `"Assign a Copilot license to jane@contoso.com"`
- Expected behavior: Check availability → confirm Jane is not already licensed → present summary → on confirmation, assign license → send welcome email. No waitlist update.

**Example 2: Assignment from waitlist**
- User request: `"Assign a license to John from the waitlist"` (John is SP ID 4)
- Expected behavior: Check availability → confirm John (SP ID 4) is not licensed → present summary including waitlist origin and SP ID → on confirmation, assign license → update waitlist status using SP ID 4 → send welcome email.

**Example 3: Already licensed**
- User request: `"Assign a license to alice@contoso.com"`
- Expected behavior: Check availability → check Alice's license → Alice is already licensed → present options (update waitlist status / send welcome email / cancel) → execute only what the operator selects → stop.

**Example 4: No licenses available**
- User request: `"Assign a license to bob@contoso.com"`
- Expected behavior: Check availability → 0 available → present alternatives (waitlist, contact admin, welcome email if applicable) → stop. Do not proceed to user identification.

**Example 5: License reassignment**
- User request: `"Move the license from Alice to Bob"` / `"Reassign Alice's license to Bob"`
- Expected behavior: Confirm both users → present reassignment summary → on confirmation, remove from Alice → assign to Bob → update waitlist if Bob was on it → send Bob a welcome email.

**Example 6: Bulk reassignment**
- User request: `"Assign all available licenses to the waitlist"` / `"Bulk assign to the queue"`
- Expected behavior: Get available count (e.g., 3) → get waitlist top 3 by request date → present bulk summary table → on confirmation, process each user in order (check → assign → update waitlist → email) → show final outcome table.

## Notes

- The `statusChangeNotes` field in waitlist updates should always reference CLARA as the source of the action and include whether the user was already in the Entra ID group or was added during the operation.
- Welcome email availability depends on whether the **Clara Communication** package has been imported. If it has not been deployed, skip the email step and note its absence in the outcome summary.
- For bulk operations, process users strictly in priority order (oldest waitlist request date first) to ensure fairness. Do not reorder based on other attributes without explicit operator instruction.
- If the operator cancels mid-bulk after some assignments have already been made, inform them of what was completed before the cancellation and stop immediately — do not roll back completed assignments.
