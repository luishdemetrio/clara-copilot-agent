---
name: "license-inventory"
description: "Retrieves and presents M365 Copilot license availability including total, assigned, and available counts."
---

## Context

- The organization manages M365 Copilot licenses centrally via Microsoft 365.
- License availability must be checked before any assignment or reassignment action.
- License data is returned directly from Microsoft Graph (`GET /v1.0/subscribedSkus`) via the **Clara Graph APIs - M365 Copilot License Overview** tool. The raw response follows the Graph `subscribedSku` schema:
  - `prepaidUnits.enabled` — total licenses purchased and active
  - `consumedUnits` — licenses currently assigned to users
  - Available licenses are derived as: `prepaidUnits.enabled − consumedUnits`
- Low license counts should trigger proactive alerts to the operator.

## Instructions

1. Call the tool **Clara Graph APIs - M365 Copilot License Overview**.
2. From the response, extract the Copilot SKU entry (look for a `skuPartNumber` containing `COPILOT` or `Microsoft_365_Copilot`) and read:
   - `prepaidUnits.enabled` → total licenses purchased
   - `consumedUnits` → licenses currently assigned
   - Available = `prepaidUnits.enabled − consumedUnits`
3. Present the data in a clear summary:
   - 📊 **Total:** {prepaidUnits.enabled}
   - ✅ **Assigned:** {consumedUnits}
   - 🟢 **Available:** {prepaidUnits.enabled − consumedUnits}
4. If available licenses ≤ 5, add a ⚠️ warning: "License pool is running low. Consider reviewing inactive users or requesting additional licenses."
5. If available licenses = 0, add a 🚨 alert: "No licenses available. Assignment is not possible until licenses are freed or purchased."

## Constraints

- Always retrieve fresh data — never use cached or assumed values from earlier in the conversation.
- Do not proceed with any assignment action if this check has not been performed first in the current turn.
- Present numbers exactly as returned by the API — do not round or estimate.
- If the response contains multiple SKUs, only report figures for the M365 Copilot SKU. Do not aggregate across unrelated SKUs.
- If the tool returns an error or an empty SKU list, surface the failure clearly: "I was unable to retrieve license data. Please check the Clara Graph APIs connection."

## Examples

**Example 1: Standard license overview**
- User request: `"Show me the Copilot license overview"` / `"How many Copilot licenses do we have?"` / `"Check license availability"`
- Expected behavior: Call **Clara Graph APIs - M365 Copilot License Overview**, extract the Copilot SKU, and present the total/assigned/available summary with the appropriate alert if low.

**Example 2: Pre-assignment check**
- User request: `"Assign a Copilot license to John"`
- Expected behavior: Before proceeding with assignment, call **Clara Graph APIs - M365 Copilot License Overview** to confirm at least one license is available. If available ≥ 1, proceed with the assignment flow. If available = 0, respond with the 🚨 alert and do not attempt assignment.

**Example 3: No licenses available**
- User request: `"Show me the license summary"`
- Expected behavior: Call the tool. If `prepaidUnits.enabled − consumedUnits = 0`, present the summary and add: "🚨 No licenses available. Assignment is not possible until licenses are freed or purchased."

**Example 4: Low license warning**
- User request: `"How many licenses are left?"`
- Expected behavior: Call the tool. If available ≤ 5 but > 0, present the summary and add: "⚠️ License pool is running low. Consider reviewing inactive users or requesting additional licenses."

## Notes

- The underlying Graph API endpoint (`GET /v1.0/subscribedSkus`) returns all license SKUs in the tenant, not just Copilot. Always filter to the relevant Copilot SKU before presenting figures.
- `prepaidUnits` also contains a `suspended` and `warning` count alongside `enabled` — use only `enabled` for the total, as suspended units are not available for assignment.
- This skill covers **reporting only**. Actual license assignment is handled by the Individual License Provisioning or Entra ID Group License Provisioning skill.
- If the operator asks for a breakdown by country, department, or office location, that is handled by the **license-grouped-count** skill, not this one.
