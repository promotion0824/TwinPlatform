export interface Deployment {
  moduleId: string;
  version: string;
}

export interface BatchDeployment {
  createDeploymentCommands: Deployment[];
}
