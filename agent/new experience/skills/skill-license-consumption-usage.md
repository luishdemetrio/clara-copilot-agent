---
name: "license-consumption-usage"
description: "Retrieves and analyzes M365 Copilot license usage, including user activity ranking, license distribution grouped by organizational attributes, and cross-referencing with user profile data."
---

## Context

- License usage data is stored in the `clara_copilotlicensetracking` Dataverse table as **daily snapshots** — each user has multiple rows, one per day. Deduplication is always required.
- User profile data (department, usage location, job title, office location) is stored in the `cr41e_clarauserprofile` Dataverse table, synced daily from Microsoft Entra ID.
- The common key between both tables is the user's UPN (`clara_userprincipalname` in the activity table, `cr41e_clara_upn` in the profile table).
- This skill covers three related capabilities: **activity ranking**, **grouped license counts**, and **cross-dimensional analysis** combining both tables. Use the appropriate action based on what the user is asking.

---

## 🛠 Action 1: Rank Users by Activity Level

Use when the user asks to find least/most active users, identify inactive users, or analyze usage patterns.

### Step 1 — Fetch data

Call **Query M365 Copilot User Activity** to query `clara_copilotlicensetracking`.

- Default timeframe: **30 days** (use user-specified value if provided)
- Retrieve: `clara_userprincipalname`, `clara_displayname`, `clara_lastactivitydate`, `clara_reportrefreshdate`

### Step 2 — Deduplicate users

The table contains multiple rows per user (daily snapshots). For each unique `clara_userprincipalname`, keep **only the row with the most recent `clara_reportrefreshdate`** and discard all older rows.

> ⚠️ **Critical:** Never return multiple rows for the same user. Validate that each email appears exactly once before presenting results.

### Step 3 — Sort

**Least active users:**
1. `NULL` `clara_lastactivitydate` first (never used Copilot = most inactive)
2. Oldest `clara_lastactivitydate` next (ascending)
3. Most recent `clara_lastactivitydate` last

**Most active users:**
1. Exclude users with `NULL` `clara_lastactivitydate`
2. Most recent `clara_lastactivitydate` first (descending)
3. Oldest `clara_lastactivitydate` last

### Step 4 — Return results

- Default: return **10 users** (use user-specified number if provided)
- Take the first N users from the sorted list after deduplication

### Output format

**Least active:**
```
Top [N] Least Active Users (Last [X] Days)

1. [Name] ([Email])
   Last Activity: Never used

2. [Name] ([Email])
   Last Activity: [Date] ([X] days ago)

Report Period: [Start Date] to [End Date]
```

**Most active:**
```
Top [N] Most Active Users (Last [X] Days)

1. [Name] ([Email])
   Last Activity: [Date] ([X] days ago)

Report Period: [Start Date] to [End Date]
```

---

## 🛠 Action 2: Group Licensed Users by Organizational Attribute

Use when the user asks to group, count, break down, or see the distribution of licensed users by a specific attribute — for example: "how many licenses per country", "group by department", "show license count by job title", "distribution by office location".

> Do not use this action for retrieving individual user details or for attributes other than the four listed below.

### Step 1 — Map the user's request to a column

| User says | FetchXML column |
|---|---|
| "usage location" / "country" / "region" | `cr41e_clara_usagelocation` |
| "department" | `cr41e_clara_department` |
| "job title" / "role" | `cr41e_clara_jobtitle` |
| "office location" | `cr41e_clara_officelocation` |

If the user requests grouping by any other attribute, do not attempt the query — ask for clarification.

### Step 2 — Call Get License Grouped Count

Call **Get License Grouped Count** using the following FetchXML template, replacing `{GROUP_BY_COLUMN}` with the mapped column name:

```xml
<fetch aggregate="true">
  <entity name="cr41e_clarauserprofile">
    <attribute name="{GROUP_BY_COLUMN}" alias="groupvalue" groupby="true" />
    <attribute name="cr41e_clara_upn" alias="total" aggregate="count" />
  </entity>
</fetch>
```

Example — grouping by department:

```xml
<fetch aggregate="true">
  <entity name="cr41e_clarauserprofile">
    <attribute name="cr41e_clara_department" alias="groupvalue" groupby="true" />
    <attribute name="cr41e_clara_upn" alias="total" aggregate="count" />
  </entity>
</fetch>
```

### Step 3 — Present results

Present as a ranked table from highest to lowest count, including percentage of total where helpful. Example:

```
Licensed Users by Department

  Department          | Users | Share
  ------------------- | ----- | -----
  Engineering         |  142  | 34%
  Sales               |   98  | 24%
  Marketing           |   61  | 15%
  ...
  Total               |  412  | 100%
```

---

## 🛠 Action 3: Cross-Dimensional Analysis (Activity + Profile)

Use when the user asks a question that combines activity level with an organizational dimension — for example: "which department has the lowest Copilot adoption?", "show inactive users in Sales", "compare usage between Brazil and the US", "how many licensed users in Engineering have never used Copilot?".

### Step 1 — Fetch activity data

Call **Query M365 Copilot User Activity**, deduplicate as in Action 1, and retain each user's most recent `clara_lastactivitydate`.

### Step 2 — Fetch profile data

Call **Clara User Profile Context** to retrieve `cr41e_clara_upn`, `cr41e_clara_department`, `cr41e_clara_usagelocation`, `cr41e_clara_jobtitle`, `cr41e_clara_officelocation`, and `cr41e_displayname`.

### Step 3 — Join on UPN

Cross-reference both datasets using `clara_userprincipalname` = `cr41e_clara_upn` as the common key.

### Step 4 — Apply the user's filter and aggregate

- **Inactivity threshold:** a user is considered inactive if `clara_lastactivitydate` is NULL or older than 30 days (use user-specified value if provided).
- Aggregate totals and compute percentages where relevant.
- Clearly distinguish between **licensed users** (present in the profile table) and **active users** (those with recent activity in the usage table).
- If a department or country has licensed users but zero activity, flag them explicitly as **fully inactive**.
- When comparing dimensions, present results ranked from most to least active.
- Always mention the `clara_reportrefreshdate` so the user understands data recency.

---

## Guidelines

- Always deduplicate activity data before presenting any results — the most common error is returning N rows of the same user from different daily snapshots.
- Use defaults (30 days, top 10) unless the user specifies otherwise.
- Present numbers exactly as returned — do not round or estimate counts.
- For grouped counts, only use the four supported FetchXML columns. Never invent or substitute column names.
- When reporting departments or countries, include totals and percentages where they add context.
- Always note the report refresh date so the user understands how current the data is.

## Examples

**Example 1: Least active users (default)**
- User request: `"Show me the 5 least active users"` / `"Who hasn't been using Copilot?"`
- Expected behavior: Call **Query M365 Copilot User Activity**, deduplicate, sort NULL first then oldest date, return top 5 with name, email, and last activity date.

**Example 2: Most active users with custom timeframe**
- User request: `"Show 10 most active users from the last 60 days"`
- Expected behavior: Call **Query M365 Copilot User Activity** with 60-day window, deduplicate, exclude NULLs, sort most recent first, return top 10.

**Example 3: Inactivity filter**
- User request: `"Who hasn't used Copilot in 45 days?"`
- Expected behavior: Call **Query M365 Copilot User Activity**, deduplicate, filter for `clara_lastactivitydate < (today − 45 days)` OR NULL, sort NULL first then oldest, return all matching users.

**Example 4: Group by country**
- User request: `"How many licenses do we have per country?"` / `"Group users by usage location"`
- Expected behavior: Call **Get License Grouped Count** with `cr41e_clara_usagelocation`, present ranked table with counts and percentages.

**Example 5: Group by department**
- User request: `"Show license count by department"`
- Expected behavior: Call **Get License Grouped Count** with `cr41e_clara_department`, present ranked table.

**Example 6: Unsupported grouping attribute**
- User request: `"Group users by manager"`
- Expected behavior: Do not attempt the query. Respond: "I can group licensed users by usage location, department, job title, or office location. Grouping by manager is not currently supported."

**Example 7: Department-level inactivity analysis**
- User request: `"Which department has the lowest Copilot adoption?"` / `"Show inactive users in Sales"`
- Expected behavior: Call **Query M365 Copilot User Activity** (deduplicate) + **Clara User Profile Context**, join on UPN, group by department, rank by percentage of active users ascending, flag any fully inactive departments.

**Example 8: Country comparison**
- User request: `"Compare usage between Brazil and the United States"`
- Expected behavior: Call both tools, join on UPN, filter for the two countries, compute active vs. licensed counts for each, present side-by-side comparison with percentages and data refresh date.

## Notes

- **Deduplication is mandatory.** The `clara_copilotlicensetracking` table stores one row per user per day. Failure to deduplicate will result in the same user appearing multiple times in ranked results.
- **No results found:** if the query returns empty for the requested timeframe, respond: "No records found in `clara_copilotlicensetracking` for the last [N] days. Try expanding the timeframe to 60 or 90 days."
- **Column name errors:** all columns in the activity table use the `clara_` prefix; all columns in the profile table use the `cr41e_clara_` prefix. Do not mix them.
- **Profile data recency:** the `cr41e_clarauserprofile` table is synced daily from Entra ID — it reflects the state of the directory as of the last sync, not real-time.
- **Licensed vs. active:** a user being in the profile table means they are licensed; a user having recent `clara_lastactivitydate` means they are active. These are distinct — always report both when comparing adoption.
