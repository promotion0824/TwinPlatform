export interface UpdateConnector {
  id: string;
  deviceName: string;
  ioTHubName: string;
  isAutoDeploy: boolean;
  environment: string;
}
