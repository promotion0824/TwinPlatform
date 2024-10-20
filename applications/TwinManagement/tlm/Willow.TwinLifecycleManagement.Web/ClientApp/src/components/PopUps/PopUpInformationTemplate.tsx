import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import LightbulbCircleIcon from '@mui/icons-material/LightbulbCircle';

interface IPopUpInformationTemplate {
  isCurrentlyOpen: boolean;
  onOpenChanged: React.Dispatch<React.SetStateAction<boolean>>;
  informationMessage: string;
}

export const PopUpInformationTemplate = (props: IPopUpInformationTemplate) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));

  const handleClose = () => {
    props.onOpenChanged(false);
  };

  return (
    <>
      <Dialog
        fullScreen={fullScreen}
        open={props.isCurrentlyOpen}
        onClose={handleClose}
        aria-labelledby="responsive-dialog-title"
      >
        <DialogTitle id="responsive-dialog-title">
          {<LightbulbCircleIcon />}
          {`   Informational message:`}
        </DialogTitle>
        <DialogContent>
          <Typography variant="subtitle1">{`${props.informationMessage}`}</Typography>
        </DialogContent>
        <DialogActions>
          <Button
            autoFocus
            onClick={handleClose}
            sx={{ maxWidth: '90%', backgroundColor: theme.palette.secondary.dark }}
            variant="contained"
            size="large"
          >
            CLOSE
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
