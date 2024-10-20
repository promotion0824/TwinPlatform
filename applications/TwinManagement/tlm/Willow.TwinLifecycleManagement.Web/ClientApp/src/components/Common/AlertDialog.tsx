import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';

export default function AlertDialog({
  open,
  title,
  content,
  actionButtons,
  onClose,
  contentSx = { width: '600px' },
  titleSx = {},
}: {
  open: boolean;
  title: string;
  content: any;
  onSubmit: () => void;
  actionButtons: any;
  onClose: () => void;
  width?: string;
  titleSx?: any;
  contentSx?: any;
}) {
  return (
    <div>
      <Dialog open={open} onClose={onClose}>
        <DialogTitle
          sx={{ ...titleSx, font: 'normal 20px/32px Poppins !important', letterSpacing: '0.15px', color: '#FFFFFF' }}
        >
          {title}
        </DialogTitle>
        <DialogContent sx={contentSx}>{content}</DialogContent>
        <DialogActions>{actionButtons}</DialogActions>
      </Dialog>
    </div>
  );
}
