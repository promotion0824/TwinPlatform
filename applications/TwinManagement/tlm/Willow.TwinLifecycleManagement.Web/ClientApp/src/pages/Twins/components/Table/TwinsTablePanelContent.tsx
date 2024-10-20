import { useState } from 'react';
import styled from '@emotion/styled';
import TwinRelationships from './RelationshipsTable';
import { ITwinWithRelationships, BasicDigitalTwin, SourceType } from '../../../../services/Clients';
import useGetTwinById from '../../hooks/useGetTwinById';
import TwinEditor from './TwinEditor/TwinEditor';
import { useTwins } from '../../TwinsProvider';
import { Loader, Panel, PanelGroup, Tabs } from '@willowinc/ui';

export default function TwinsTablePanelContent({ row }: { row: ITwinWithRelationships }) {
  const [selectedTab, setSelectedTab] = useState<string>('properties');

  const { twin } = row;

  const { $dtId: id } = twin as BasicDigitalTwin;

  const { data, isLoading } = useGetTwinById(id!, undefined, undefined, SourceType.AdtQuery);

  const { incomingRelationships = [], outgoingRelationships = [] } = data || {};

  const { apiRef } = useTwins();
  return (
    <StyledPanelGroup>
      <Panel
        tabs={
          <Tabs value={selectedTab} onTabChange={(value: string | null) => setSelectedTab(value!)}>
            <Tabs.List>
              <Tabs.Tab value="properties">Properties</Tabs.Tab>

              <Tabs.Tab value="outgoingRelationships">
                {isLoading ? <Loader /> : `Outgoing Relationships (${outgoingRelationships.length})`}
              </Tabs.Tab>

              <Tabs.Tab value="incomingRelationships">
                {isLoading ? <Loader /> : `Incoming Relationships (${incomingRelationships.length})`}
              </Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="properties">
              {selectedTab === 'properties' && <TwinEditor key={id} twinData={twin!} twinId={id} apiRef={apiRef} />}
            </Tabs.Panel>
            <Tabs.Panel value="outgoingRelationships">
              {selectedTab === 'outgoingRelationships' && (
                <TwinRelationships relationshipsData={outgoingRelationships} type={'outgoing'} />
              )}
            </Tabs.Panel>

            <Tabs.Panel value="incomingRelationships">
              {selectedTab === 'incomingRelationships' && (
                <TwinRelationships relationshipsData={incomingRelationships} type={'incoming'} />
              )}
            </Tabs.Panel>
          </Tabs>
        }
      />
    </StyledPanelGroup>
  );
}

const StyledPanelGroup = styled(PanelGroup)({ margin: '1rem', width: 'unset !important' });
