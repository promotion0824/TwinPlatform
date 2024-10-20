import {Connector} from "./Connector";

export interface EditConnectorDialogProps {
  open: boolean,
  closeHandler: () => void,
  connector: Connector,
  onConfirm: (change: boolean) => void,
  setOpenError: (open: boolean) => void;
}
