import { useState } from 'react';
import styled from '@emotion/styled';
import useGetTwinById from '../../../Twins/hooks/useGetTwinById';
import TwinEditor from '../../../Twins/components/Table/TwinEditor/TwinEditor';
import { Loader, Panel, PanelGroup, Tabs, Button } from '@willowinc/ui';
import { useSearchParams } from 'react-router-dom';
import { SourceType } from '../../../../services/Clients';

export default function ConflictsTablePanelContent({ row }: { row: any }) {
  const [selectedTab, setSelectedTab] = useState<string>('properties');

  const { willowTwinId, changedProperties = [] } = row;

  const { data, isLoading, isSuccess, isFetching, isFetched } = useGetTwinById(
    willowTwinId!,
    undefined,
    false,
    SourceType.AdtQuery
  );

  const { twin } = data || {};

  const isInvalidTwin = !data;

  return (
    <StyledPanelGroup>
      <Panel
        tabs={
          <Tabs value={selectedTab} onTabChange={(value: string | null) => setSelectedTab(value!)}>
            <Tabs.List>
              <Tabs.Tab value="properties">Properties</Tabs.Tab>
              <Tabs.Tab value="mappedConflicts">Mapped Conflicts</Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="properties">
              {selectedTab === 'properties' && (
                <>
                  {!isLoading && isInvalidTwin && <InvalidTwin />}
                  {isFetching && !isFetched && (
                    <Container>
                      <Loader size="lg" variant="dots" />
                    </Container>
                  )}
                  {!isInvalidTwin && isSuccess && <TwinEditor noEdit twinData={twin} twinId={willowTwinId} />}
                </>
              )}
            </Tabs.Panel>
            <Tabs.Panel value="mappedConflicts">
              {selectedTab === 'mappedConflicts' && (
                <MappedConflictsContainer>
                  <CodeBlock>
                    {changedProperties.map((conflict: any) => {
                      return <p key={`${JSON.stringify(conflict)}`}>{JSON.stringify(conflict, null, ' ') + ','}</p>;
                    })}
                  </CodeBlock>
                </MappedConflictsContainer>
              )}
            </Tabs.Panel>
          </Tabs>
        }
      />
    </StyledPanelGroup>
  );
}

function InvalidTwin() {
  const [searchParams, setSearchParams] = useSearchParams();

  function handleSwitchSource(source: string) {
    setSearchParams({ ...Object.fromEntries(searchParams.entries()), source: 'adt' });
  }
  const source = searchParams.get('source');

  const isADT = source?.toLowerCase() === 'adt';
  const isAdx = source?.toLowerCase() === 'adx';

  return (
    <Container>
      <p>Twin does not exist in {isADT ? 'ADT' : isAdx ? 'ADX' : 'ADT'}</p>
      {isAdx ? (
        <Button
          kind="secondary"
          onClick={() => {
            handleSwitchSource('adt');
          }}
        >
          Switch to ADT source
        </Button>
      ) : (
        <Button
          kind="secondary"
          onClick={() => {
            handleSwitchSource('adx');
          }}
        >
          Switch to ADX source
        </Button>
      )}
    </Container>
  );
}
const Container = styled('div')({
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100%',
  padding: '10rem',
});

const MappedConflictsContainer = styled('div')({ padding: '1rem' });

const StyledPanelGroup = styled(PanelGroup)({ margin: 16, width: 'unset !important' });

const CodeBlock = styled('code')({
  display: 'block',
  padding: '1rem',
  color: 'unset !important',
  backgroundColor: 'black',
  borderRadius: '4px',
  overflow: 'auto',
  whiteSpace: 'pre-wrap',
  maxHeight: 320,
  fontSize: '1em',
});
