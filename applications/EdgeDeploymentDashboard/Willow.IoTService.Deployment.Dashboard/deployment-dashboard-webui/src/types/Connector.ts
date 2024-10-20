export interface Connector {
  id: string;
  name: string;
  deviceName: string;
  moduleType: string;
  ioTHubName: string;
  environment: string;
  dateTimeApplied: string;
  deploymentId: string;
  version: string;
  assignedBy: string;
  status: string;
  statusMessage: string;
  isAutoDeployment: boolean;
  platform: string;
}
