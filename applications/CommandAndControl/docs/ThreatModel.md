# Threat Model

Key
---
* :recycle: <span style="color:#ce1ede">Out-of-scope, someone else at Willow owns this</span>
* :white_check_mark: <span style="color:#1ece1e">Risks are low and/or adequately mitigated</span>
* :purple_heart: <span style="color:#cece1e">Improvements are possible</span>
* :fire: <span style="color:#ef1c1c">Danger</span>

The ActiveControl threat model is based on STRIDE: Spoofing, Tampering, Repudiation, Information Disclosure, Denial Of Service, Elevation of Privilege.

User access is by the web only and all traffic is routed thus:

```mermaid
graph LR
User[User] -->|https| CF
CF[Cloudflare] -->|https| ACE
ACE[Azure Container Environment Ingress] -->|http| AC
AC[ActiveControl]
```

Data Flow Diagram
====

```mermaid
graph TD
W -->|RW| SQL
ADX -->|RO| W
W --> SB2[(SB Topic\nSend Command)] --> EC
EC --> SB1[(SB Topic\nCommand\nResponse)] --> W[ActiveControl API]
RE -->|Creates Request| W
RE[Activate Technology]
SQL[(SQL)]
ADX[ADX]
ADX[ADX]
EC[Edge Connector]
```

----
Development
====

|STRIDE | Discussion
|----|----|
|Spoofing | <span style="color:#1ece1e">Low risk, Azure DevOps and GitHub repository policies are in place. :white_check_mark:</span>
|Tampering | <span style="color:#cece1e">Developer could introduce malicious code. Mitigated by ... PR process. Developers should not have write access to ADX production systems. :recycle:</span>
| Repudiation | <span style="color:#1ece1e">Git history and PR process. :white_check_mark:</span>
| Information Disclosure | <span style="color:#1ece1e">No need for production data access during development. Commands can be created locally and executed using the lab equipment. :white_check_mark:</span>
| Denial of service | <span style="color:#1ece1e">N/A</span> :white_check_mark:
| Elevation of privilege | <span style="color:#ce1ede">DevOps controls. :recycle:</span>

> __Note__ We should develop DDK as a test environment too so developers have no access to production data.

----
Build process
====
| STRIDE | Discussion
|----|----|
| Spoofing | N/A
| Tampering | <span style="color:#ce1ede">Bad base image from container repository, Malicious NPM package. DevOps pipeline responsibility to enforce required source checks. :recycle:</span>
| Repudiation | <span style="color:#ce1ede">DevOps logs :recycle:<mark>
| Information Disclosure | N/A :white_check_mark:
| Denial of service | N/A :white_check_mark:
| Elevation of privilege | <span style="color:#ce1ede">Someone edits the YAML file outside the PR process. Prevented by roles and CODEOWNERS in GitHub. :recycle:</span>

----
Deployment process
====
<span style="color:#ce1ede">Platform Team owns this issue. Must ensure containers come from trusted sources, authentication and authorization for deployment admins etc. This merits a separate review. :recycle: :fire:</span>.

| STRIDE | Discussion
|----|----|
| Spoofing | Low risk, GitHub groups and roles used to control deployment authorisation.
| Tampering | <span style="color:#ce1ede">Bad image from the container repository. :recycle:</span>
| Repudiation | <span style="color:#ce1ede">GitHub Actions logs :recycle:<mark>
| Information Disclosure | N/A :white_check_mark:
| Denial of service | N/A :white_check_mark:
| Elevation of privilege | <span style="color:#ce1ede">Someone edits the YAML file outside the PR process. Prevented by roles and CODEOWNERS in GitHub. :recycle:</span>

----
Authentication
====
<span style="color:#1ece1e">ADB2C provides all authentication using MSAL libraries. All API calls are checked, 401 errors cause an immediate redirect to the login page. We rely on MSAL for all anti-tampering measures. We do check the JWT Token Authority and Scopes. We rely on ADB2C and AD for any logging of authentication activities.</span> <span style="color:#cece1e">Code review recommended for how MSAL is used and configured.</span> :white_check_mark:

> __Note__ Code review recommended for how MSAL is used and configured.

----
User Management
====
<span style="color:#1ece1e">Azure Portal Active Directory is used for User Management including Group/List membership. Low risk.</span> :white_check_mark:

----
Authorization
====
<span style="color:#1ece1e">ActiveControl uses DOTNETCORE policy based access control. Implementation of the policies is handled by User Management. Default assignments of permissions to roles is handled. </span> :white_check_mark:

| STRIDE | Discussion
|----|----|
| Spoofing | <span style="color:#1ece1e">All policies are checked on back-end.</span> :white_check_mark:
| Tampering | <span style="color:#1ece1e">On the front-end, the disabled button or hidden controls could be tampered with but achieves nothing as back-end controls authorization. :white_check_mark:</span>
| Repudiation | <span style="color:#1ece1e">We rely on AppInsights logging. :white_check_mark:</span>
| Information disclosure | <span style="color:#cece1e">Auth failures are reported to the web UI but only minimal details are supplied as to how to address, e.g. "Contact your admin for access to this feature". Needs to be verified. :white_check_mark:</span>
| Denial of service | <span style="color:#1ece1e">N/A - policy decisions are implemented in code taking microseconds to evaluate, AD groups are cached for five minutes which rate limits that. :white_check_mark:</span>
| Elevation of privilege | <span style="color:#ce1ede">Roles and permissions are provided by User Management. :recycle:</span> <div style="color:#1ece1e">Default assignments of permissions to roles are stored by the app in settings and sent to User Management. Tampering during development / deployment would be required to modify these. :white_check_mark:</div>


Endpoint Methods
--
| STRIDE | Discussion
|----|----|
| Spoofing | <span style="color:#1ece1e">ASPNETCORE checks the Authorize header token on all requests. A valid JWT from a recognized Issuer/Authority must be present. :white_check_mark:</span>
| Tampering | <span style="color:#1ece1e">All endpoint methods are protected with Authorize attributes except app configuration. :white_check_mark:</span>
| Repudiation | <span style="color:#1ece1e">AppInsights logging . :white_check_mark:</span>
| Information Disclosure | <span style="color:#cece1e">Exception details should not be reported with 500 errors in production. :purple_heart:</span>
| Denial of service | <span style="color:#ce1ede">Protection provided by Cloudflare. :recycle:</span>

> __Note__ Check that detailed exceptions are not shown in production browser logs or UI

---
ActiveControl Web App
====
| STRIDE | Discussion
|----|----|
| Spoofing | <span style="color:#1ece1e">Any user with approve/execute rights can approve and execute any command. :white_check_mark:</span>
| Tampering | <span style="color:#1ece1e">Any user with approve/execute rights can approve and execute any command. All inputs use standard react components that _should_ protect against script injection. :white_check_mark:</span>
| Repudiation | <span style="color:#1ece1e">User actions are logged using the standard audit logging process to AppInsights. User actions are separaetly logged to the app database for display within the app. :white_check_mark:</span>
| Information disclosure | <span style="color:#1ece1e">User has acces to view all requests, commands and activity logs, so no risk of disclosing information to which they should not have access. This may change in the future. :white_check_mark:</span>
| Denial of service | <span style="color:#ce1ede">Protection provided by Cloudflare. :recycle:</span>
| Elevation of privilege | <span style="color:#1ece1e">Low risk, ActiveControl runs under Managed Identity. See authentication and authorization for user privilege discussion. :white_check_mark:</span>

----
Edge Connector
====
| STRIDE | Discussion
|----|----|
| Spoofing | <span style="color:#1ece1e">Low risk, the edge connector only sends success / failure notifications. :white_check_mark:</span>
| Tampering | <span style="color:#1ece1e">Low risk. Service Bus messages are not signed, but access to the topic is Azure RBAC. :white_check_mark:</span>
| Repudiation | <span style="color:#1ece1e">N/A, Service Bus logging may include some details :white_check_mark:</span>
| Information disclosure | <span style="color:#1ece1e">Low risk, Service bus messages are low-information content. :white_check_mark:</span>
| Denial of service | <span style="color:#cece1e">Low risk, the edge connector processes messages sequentially, and is unlikely to overload the customer network. :purple_heart:</span>
| Elevation of privilege | <span style="color:#1ece1e">Low risk, runs as a container in the Edge Runtime on either a Trustbox or customer-managed VM. :white_check_mark:</span>

----
Logging
====
| STRIDE | Discussion
|----|----|
| Spoofing | <span style="color:#1ece1e">App Insights permissions. Deployment issue. :white_check_mark:</span>
| Tampering | <span style="color:#1ece1e">App Insights permissions. Deployment issue. :white_check_mark:</span>
| Repudiation | <span style="color:#1ece1e">App Insights permissions. Deployment issue. :white_check_mark:</span>
| Information Disclosure | <span style="color:#cece1e">We may be logging more than we should in terms of user identity but we have no PII beyond email and name. What are log retention policies and who has access to them? Dev Ops issue. :purple_heart:</span>
| Denial of service | <span style="color:#1ece1e">Unlikely, App Insights throttles logging. A user could cause that by making a lot of web requests, we need to protect against that with rate limiting probably at a higher level than here. :white_check_mark:</span>
| Elevation of privilege | <span style="color:#1ece1e">N/A :white_check_mark:</span>

> __Note__ Check we aren't logging PII or retaining it for too long.

----
SQL Database
====
<span style="color:#cece1e">Deployment limits database access to the Managed Identity of the container app. The current implementation updates the database schema from within the app, requiring the app to have the `db_owner` role. :purple_heart: :fire:</span>

<span style="color:#1ece1e">Entity Framework Core is used for all data access giving good protection against SQL injection. :white_check_mark:
</span>


ADX
====
<span style="color:#ce1ede">Data reader only access secured by Azure Managed Identity. :recycle: </span>
