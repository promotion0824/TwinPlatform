import Box from '@mui/material/Box';
import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import * as React from 'react';
import {useState} from 'react';
import {EditConnectorDialogProps} from '../types/EditConnectorDialogProps';
import ConnectorDialogTabConfiguration from './ConnectorDialogTabConfiguration';
import ConnectorDialogTabManifest from './ConnectorDialogTabManifest';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const {children, value, index, ...other} = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{p: 3}}>
          {children}
        </Box>
      )}
    </div>
  );
}

function a11yProps(index: number) {
  return {
    id: `simple-tab-${index}`,
    'aria-controls': `simple-tabpanel-${index}`,
  };
}

export default function ConnectorDialogTabs(props: EditConnectorDialogProps) {
  const {connector, closeHandler, onConfirm, setOpenError} = props;
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <Box sx={{width: 475}}>
      <Box sx={{borderBottom: 1, borderColor: 'divider'}}>
        <Tabs value={tabValue} onChange={handleTabChange} aria-label="configuration manifest tab">
          <Tab label="CONFIGURATION" {...a11yProps(0)} />
          <Tab label="DEPLOYMENT" {...a11yProps(1)} />
        </Tabs>
      </Box>
      <TabPanel value={tabValue} index={0}>
        <ConnectorDialogTabConfiguration
          connector={connector}
          open={false}
          closeHandler={closeHandler}
          onConfirm={onConfirm}
          setOpenError={setOpenError}
        />
      </TabPanel>
      <TabPanel value={tabValue} index={1}>
        <ConnectorDialogTabManifest
          connector={connector}
          open={false}
          closeHandler={closeHandler}
          onConfirm={onConfirm}
          setOpenError={setOpenError}
        />
      </TabPanel>
    </Box>
  );
}
