import {Icon, IconButton} from '@willowinc/ui';
import Dialog from '@mui/material/Dialog';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import {styled} from '@mui/material/styles';
import * as React from 'react';
import {EditConnectorDialogProps} from '../types/EditConnectorDialogProps';
import ConnectorDialogTabs from './ConnectorDialogTabs';

const BootstrapDialog = styled(Dialog)(({theme}) => ({
  '& .MuiDialogContent-root': {
    padding: theme.spacing(2),
  },
  '& .MuiDialogActions-root': {
    padding: theme.spacing(1),
  },
}));

interface DialogTitleProps {
  id: string;
  children?: React.ReactNode;
  onClose: () => void;
}

const BootstrapDialogTitle = (props: DialogTitleProps) => {
  const {children, onClose, ...other} = props;

  return (
    <DialogTitle sx={{m: 0, p: 2}} {...other}>
      {children}
      {onClose ? (
        <IconButton
          aria-label="close"
          onClick={onClose}
          icon="close"
          style={{
            position: 'absolute',
            right: 8,
            top: 8,
          }}
        >
        </IconButton>
      ) : null}
    </DialogTitle>
  );
};

export default function ConnectorDialog(props: EditConnectorDialogProps) {
  const {open, closeHandler, connector, onConfirm, setOpenError} = props;
  return (
    <div>
      <BootstrapDialog
        open={open}
        onClose={closeHandler}
        aria-labelledby="customized-dialog-title"
        maxWidth='sm'
        fullWidth={true}
      >
        <BootstrapDialogTitle id="customized-dialog-title" onClose={closeHandler}>
          {connector.name}
        </BootstrapDialogTitle>
        <DialogContent>
          <ConnectorDialogTabs
            connector={connector}
            open={false}
            closeHandler={closeHandler}
            onConfirm={onConfirm}
            setOpenError={setOpenError}/>
        </DialogContent>
      </BootstrapDialog>
    </div>
  );
}
