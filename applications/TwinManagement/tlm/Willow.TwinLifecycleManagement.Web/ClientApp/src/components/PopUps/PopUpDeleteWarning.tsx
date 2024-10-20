import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import { useState } from 'react';
import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Typography, useTheme, styled } from '@mui/material';

import ArrowForwardIosSharpIcon from '@mui/icons-material/ArrowForwardIosSharp';
import MuiAccordion, { AccordionProps } from '@mui/material/Accordion';
import MuiAccordionSummary, { AccordionSummaryProps } from '@mui/material/AccordionSummary';
import MuiAccordionDetails from '@mui/material/AccordionDetails';

export type DeleteWarning = { exceptions: Record<string, string>; title: string };
interface IPopUpExceptionTemplate {
  isOpen: boolean;
  onOpen: (isOpen: boolean) => void;
  errors: DeleteWarning | null;
}

export default function PopUpDeleteWarning({ isOpen, onOpen, errors }: IPopUpExceptionTemplate) {
  const theme = useTheme();

  const handleClose = () => {
    onOpen(false);
  };

  const { exceptions = {}, title = '' } = errors || {};

  return (
    <>
      <Dialog fullWidth maxWidth="sm" open={isOpen} onClose={handleClose}>
        <DialogTitle>
          <Flex>
            <WarningAmberIcon sx={{ fontSize: 28, marginTop: '-2px !important' }} />
            <TitleText>{title}</TitleText>
          </Flex>
        </DialogTitle>
        <DialogContent>
          <DialogContent style={{ margin: '5px 0 0 0', padding: 0, backgroundColor: theme.palette.secondary.dark }}>
            <OverFlowContainer>
              <CustomizedAccordions errorsObj={exceptions} />
            </OverFlowContainer>
          </DialogContent>
        </DialogContent>

        <DialogActions>
          <Button
            autoFocus
            onClick={handleClose}
            sx={{ maxWidth: '90%', backgroundColor: theme.palette.secondary.dark }}
            variant="contained"
            size="large"
            data-cy="close-button"
          >
            CLOSE
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}

const OverFlowContainer = styled('div')({ overflow: 'auto', maxHeight: 400 });

const Flex = styled('div')({ display: 'flex', alignItems: 'center', gap: 14 });
const TitleText = styled('span')({ font: "500 20px/32px 'Roboto', sans-serif", color: '#FFFFFF', letter: '0.15px' });

const CustomizedAccordions = ({ errorsObj }: { errorsObj: Record<string, any> }) => {
  const [expanded, setExpanded] = useState<string | false>();

  const handleChange = (panel: string) => (event: React.SyntheticEvent, newExpanded: boolean) => {
    setExpanded(newExpanded ? panel : false);
  };

  return (
    <div>
      {Object.entries(errorsObj).map(([key, value]) => {
        return (
          <Accordion key={key} expanded={expanded === key} onChange={handleChange(key)}>
            <AccordionSummary>
              <Typography>{key}</Typography>
            </AccordionSummary>
            <AccordionDetails>{parseResponseToJSX(value)}</AccordionDetails>
          </Accordion>
        );
      })}
    </div>
  );
};

const parseResponseToJSX = (response: any) => {
  try {
    const parsedResponse = JSON.parse(response);

    const m = parsedResponse.split('\r\n');
    return m.map((line: string) => (
      <StyledP key={line} $isEmptyLine={line === ''}>
        {line}
      </StyledP>
    ));
  } catch {
    return response;
  }
};

const StyledP = styled('p')(({ $isEmptyLine }: { $isEmptyLine: boolean }) => ({
  margin: 0,
  marginBottom: $isEmptyLine ? '0.5rem' : '0',
}));

const AccordionDetails = styled(MuiAccordionDetails)({
  borderTop: '1px solid rgba(0, 0, 0, .125)',
});

const Accordion = styled((props: AccordionProps) => <MuiAccordion disableGutters elevation={0} square {...props} />)(
  ({ theme }) => ({
    border: `1px solid ${theme.palette.divider}`,
    '&:not(:last-child)': {
      borderBottom: 0,
    },
    '&:before': {
      display: 'none',
    },
  })
);

const AccordionSummary = styled((props: AccordionSummaryProps) => (
  <MuiAccordionSummary expandIcon={<ArrowForwardIosSharpIcon sx={{ fontSize: '0.9rem' }} />} {...props} />
))(({ theme }) => ({
  backgroundColor: theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, .05)' : 'rgba(0, 0, 0, .03)',
  flexDirection: 'row-reverse',
  '& .MuiAccordionSummary-expandIconWrapper.Mui-expanded': {
    transform: 'rotate(90deg)',
  },
  '& .MuiAccordionSummary-content': {
    marginLeft: theme.spacing(1),
  },
}));
