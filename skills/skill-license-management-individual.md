---
name: "license-management-individual"
description: "Manages M365 Copilot license assignment, reassignment, and bulk reassignment for individual users via the Graph API. Covers license availability checks, already-licensed user handling, waitlist status updates, and welcome email delivery."
---

## Context

- This skill handles direct, per-user license operations via the Microsoft Graph API (`Manage User License Individual` ).
- All actions require **explicit operator confirmation** before any write operation is executed. Never proceed without it.
---

## 🛠 Action: Assign a License

> ⚠️ **Critical:** Requires explicit confirmation. Follow steps in order. Do not skip or combine steps.

### Step 1 — Check license availability

- Check license availability.

- **No licenses available →** Inform the operator.
  - **Stop here. Do not proceed.**
- **Licenses available →** Proceed to Step 2.

### Step 2 — Identify the target user

- Get the target user id.

### Step 3 — Present summary and request confirmation

Present a summary to the operator and ask for explicit confirmation before proceeding.

Do **not** proceed without explicit confirmation. If the operator selects Cancel, terminate with no further changes.

### Step 4 — Execute (only after confirmation received)

Execute each sub-step in order. Do not begin the next sub-step until the current one returns successfully.

#### 4.1 — Check existing license status

Verify whether the user already the license assigned.

#### 4.2 — Handle already-licensed users

**If the user is already licensed →** Do not proceed with assignment. Inform the operator.

**If the user is not yet licensed →** Proceed to 4.3.

#### 4.3 — Assign the license
Assign the license to the target user.





