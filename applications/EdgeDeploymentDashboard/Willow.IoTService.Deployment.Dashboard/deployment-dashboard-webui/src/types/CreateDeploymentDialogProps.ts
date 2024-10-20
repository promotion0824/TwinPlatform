import {Connector} from "./Connector";
import {CreateDialogProps} from '../types/CreateDialogProps';

export interface CreateDeploymentDialogProps extends CreateDialogProps {
  connector?: Connector
}
