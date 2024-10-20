import { Button, Panel, PanelGroup, PanelHeader } from '@willowinc/ui';
import { useCallback, useState } from 'react';
import DeploymentDialogCreateDeployment from './ConnectorCreateModule';
import DeploymentDataGrid from './DeploymentDataGrid';
import { PanelHeaderContentWithActions } from './PanelHeaderContentWithActions';

export default function Deployments(props: { setOpenError: (open: boolean) => void; }) {

  const [openDialogCreateDeployment, setOpenDialogCreateDeployment] = useState<boolean>(false);

  const handleCreateDeployment = () => {
    setOpenDialogCreateDeployment(true);
  };

  const handleCloseDialogCreateDeployment = useCallback(() => {
    setOpenDialogCreateDeployment(false);
  }, []);

  const handleSaveCreateDeployment = useCallback((change: boolean) => {
    if (change) {
      // To trigger data grid refresh after saved
      //setApiData(null);
    }
  }, []);

  return (
    <PanelGroup>
      <Panel>
        <PanelHeader>
          <PanelHeaderContentWithActions>
          <span>Deployments</span>
            <Button variant="contained" onClick={handleCreateDeployment}>
              Create Deployment
            </Button>
          </PanelHeaderContentWithActions>
        </PanelHeader>
        <DeploymentDataGrid {...props} />
      </Panel>
      <DeploymentDialogCreateDeployment
        open={openDialogCreateDeployment}
        closeHandler={handleCloseDialogCreateDeployment}
        onConfirm={handleSaveCreateDeployment}
        setOpenError={props.setOpenError}
      />
    </PanelGroup>);
}
