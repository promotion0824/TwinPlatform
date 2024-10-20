import { Button, ButtonGroup, Panel, PanelGroup, PanelHeader } from '@willowinc/ui';
import ConnectorDataGrid from './ConnectorDataGrid';
import { PanelHeaderContentWithActions } from './PanelHeaderContentWithActions';
import ConnectorCreateModule from './ConnectorCreateModule';
import { useCallback, useState } from 'react';

export default function Connectors(props: { setOpenError: (open: boolean) => void; }) {

  const [openDialogCreateModule, setOpenDialogCreateModule] = useState<boolean>(false);

  const handleCreateModule = () => {
    setOpenDialogCreateModule(true);
  };

  const handleCloseDialogCreateModule = useCallback(() => {
    setOpenDialogCreateModule(false);
  }, []);

  return (
    <PanelGroup>
      <Panel>
        <PanelHeader>
          <PanelHeaderContentWithActions>
            <span>Connectors</span>
            <ButtonGroup>
              <Button kind="primary" onClick={handleCreateModule}>Create Module</Button>
            </ButtonGroup>
          </PanelHeaderContentWithActions>
        </PanelHeader>
        <ConnectorDataGrid {...props} />
        <ConnectorCreateModule
          open={openDialogCreateModule}
          closeHandler={handleCloseDialogCreateModule}
          onConfirm={() => { }}
          setOpenError={props.setOpenError}
        />
      </Panel>
    </PanelGroup>
  );
}
