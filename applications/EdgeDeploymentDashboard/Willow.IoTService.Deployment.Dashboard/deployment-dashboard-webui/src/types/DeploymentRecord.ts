export interface DeploymentRecord {
  id: string;
  moduleId: string;
  moduleName: string;
  moduleType: string;
  name: string;
  version: string;
  assignedBy: string;
  status: string;
  statusMessage: string;
  dateTimeCreated: string;
  dateTimeApplied: string;
}
