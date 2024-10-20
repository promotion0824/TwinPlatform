import { PropsWithChildren, useRef } from 'react';
import { Box, Typography } from '@mui/material';

interface TabPanelProps {
  children?: React.ReactNode;
  index: any;
  value: any;
}

//Avoid rendering the tab content until rendered prop becomes true the first time.
//After that rendered only affects the display and therefore state of the tabs is kept.
function TabPanel(props: PropsWithChildren<TabPanelProps>) {
  const { children, value, index, ...other } = props;
  const selected = value === index;
  const rendered = useRef(selected);

  if (selected && !rendered.current) {
    rendered.current = true;
  }

  if (!rendered.current) {
    return null;
  }

  return (
    <Typography
      component="div"
      role="tabpanel"
      hidden={!selected}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}>
      <Box pt={2} pb={3}>{rendered && children}</Box>

    </Typography>
  );
}

export default TabPanel;
