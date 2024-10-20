### Single-Tenant Migration

This diagram attempts to make clear the goals (green) and their dependencies on various steps we need to complete the move to Single Tenant.

Along the way we also want to move off Kubernetes and Twin Platform Deployer (TPD) and to complete the deprecation of Auth0 that was started several years ago but never completed.

```mermaid

flowchart LR
P(Public API\nold domain\nbeing thunked) -->|dependsOn| T(Thunking\nService)


T -->|dependsOn|STP(Public API\ndeployed in\nSingle Tenant)

WApp(Willow App\nrunning in\nSingle Tenant) -->|dependsOn| ST
WApp -->|dependsOn| UM

EC(Existing\nConnectors\ncontinue to work)
EC -->|dependsOn| T


NEW[New Customers\nOn single-tenant]
NEW -.->|dependsOn| Mapped
NEW --->|dependsOn| WApp
Mapped -->|dependsOn| ST

OLD(Existing customers\nmigrated to\nSingle tenant)
OLD -->|dependsOn| WApp
OLD -->|dependsOn| P
OLD -->|dependsOn| EC

UM(User\nManagement)
UM -->|dependsOn| ST

ST(Single Tenant\nACA Environment\nusing\nNew Deployer\nhosting\nTPD\nRules Engine\nUser Management)
STP -->|dependsOn| ST

CMT[All connectors\nmigrated\nto single tenant]


A0C[Auth0\nShutdown\nComplete]
A0C -->|dependsOn| EC
A0C -->|dependsOn| M2A
A0C -->|dependsOn| CMT

M2A[M2M auth\nswitched to\nActive Directory]
M2A --->|dependsOn| ST

TOT[Turn off\nthunking service]
TOT -->|dependsOn| CMT

TPD[TPD Shutdown\nTurn off K8s]
TPD ----->|dependsOn| ST

style TPD fill:#4c4
style OLD fill:#4c5
style NEW fill:#4c4
style A0C fill:#4c4
style TOT fill:#4c4

```
