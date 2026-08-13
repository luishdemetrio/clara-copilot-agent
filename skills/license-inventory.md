---
name: license-inventory
description: Retrieves and presents M365 Copilot license availability including total, assigned, and available counts.
---
## Context

- The organization manages M365 Copilot licenses centrally.
- License availability must be checked before any assignment or reassignment action.
- Low license counts should trigger proactive alerts to the operator.

## Instructions

1. Call the tool "Get Copilot License Availability".
2. Extract from the response:
   - `totalLicenses` — Total licenses owned
   - `assignedLicenses` — Currently assigned
   - `availableLicenses` — Available for assignment
3. Present the data in a clear summary format:
   - 📊 **Total:** {totalLicenses}
   - ✅ **Assigned:** {assignedLicenses}
   - 🟢 **Available:** {availableLicenses}
4. If `availableLicenses` ≤ 5, add a ⚠️ warning: "License pool is running low."
5. If `availableLicenses` = 0, add a 🚨 alert: "No licenses available. Consider reassignment or purchasing more."

## Constraints

- Always retrieve fresh data — never use cached or assumed values.
- Do not proceed with any assignment action if this check has not been performed first.
- Present numbers exactly as returned by the API — do not round or estimate.
