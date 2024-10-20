import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Divider from '@mui/material/Divider';

export default function PopUp({
  open,
  title,
  content,
  actionButtons,
  onClose,
}: {
  open: boolean;
  title: string;
  content: any;
  actionButtons: any;
  onClose: () => void;
}) {
  return (
    <>
      <Dialog
        open={open}
        onClose={onClose}
        PaperProps={{ sx: { width: '100%', maxHeight: 'unset', maxWidth: '75vw' } }}
      >
        <DialogTitle variant="h1" style={{ fontWeight: 'normal', textAlign: 'center', margin: '0 auto' }}>
          {title}
        </DialogTitle>
        <Divider />
        <DialogContent sx={{ width: '100%%', height: '100%' }}>{content}</DialogContent>

        <DialogActions sx={{ justifyContent: 'flex-start', paddingLeft: '24px !important', gap: 1 }}>
          {actionButtons}
        </DialogActions>
      </Dialog>
    </>
  );
}
