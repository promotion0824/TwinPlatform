import {DeploymentRecord} from "./DeploymentRecord";

export interface ManifestDialogProps {
  open: boolean,
  closeHandler: () => void,
  deployment: DeploymentRecord | undefined
}
