export interface CreateDialogProps {
  open: boolean,
  closeHandler: () => void,
  onConfirm: (change: boolean) => void
  setOpenError: (open: boolean) => void;
}
