import MappingsTable from './components/MappingsTable';
import MappingsProvider, { useMappings, TabsName } from './MappingsProvider';
import styled from '@emotion/styled';
import {
  GridToolbarContainer,
  GridToolbarColumnsButton,
  GridToolbarFilterButton,
  GridToolbarExport,
  GridToolbarDensitySelector,
} from '@mui/x-data-grid-pro';
import { Tabs as WillowTabs, Badge, Popover, useDisclosure, Loader } from '@willowinc/ui';
import ApproveAcceptActionButton from './components/ApproveAcceptActionButtons';
import useKeyboardShortcuts from './hooks/useKeyboardShortcuts';
import { Status, IMappedEntry, UpdateMappedTwinRequestResponse } from '../../services/Clients';
import ConflictsTable from './components/ConflictsTable/ConflictsTable';
import useGetUpdateTwinCountRequests from './hooks/useGetUpdateTwinRequestsCount';
import PageLayout from '../../components/PageLayout';
import ApproveAcceptDropdownFilters from './components/ApproveAcceptDropdownFilters';
import CustomGridFooter from './components/CustomGridFooter';
import { useEffect } from 'react';
import { ErrorBoundary } from 'react-error-boundary';

export default function MappingsPage() {
  return (
    <MappingsProvider>
      <KeyboardShortcutWrapper>
        <PageLayout>
          <PageLayout.Header pageTitleItems={[{ title: 'Review Twins (Approve & Accept)' }]}></PageLayout.Header>

          <PageLayout.ActionBar>
            <PageLayout.ActionBar.LeftSide>
              <ApproveAcceptDropdownFilters />
            </PageLayout.ActionBar.LeftSide>

            <PageLayout.ActionBar.RightSide>
              <ApproveAcceptActionButton />
            </PageLayout.ActionBar.RightSide>
          </PageLayout.ActionBar>

          <PageLayout.MainContent>
            <Tables />
          </PageLayout.MainContent>
        </PageLayout>
      </KeyboardShortcutWrapper>
    </MappingsProvider>
  );
}

function KeyboardShortcutWrapper({ children }: { children: React.ReactNode }) {
  useKeyboardShortcuts();

  return <>{children}</>;
}

function Tables() {
  const { tabState, rowsState } = useMappings();

  return (
    <>
      {tabState[0] === 'things' && <ThingsTable rowsState={rowsState} />}
      {tabState[0] === 'points' && <PointsTable rowsState={rowsState} />}
      {tabState[0] === 'spaces' && <SpacesTable rowsState={rowsState} />}
      {tabState[0] === 'miscellaneous' && <MiscellaneousTable rowsState={rowsState} />}
      {tabState[0] === 'conflicts' && <ConflictsTable1 rowsState={rowsState} />}
    </>
  );
}

function ConflictsTable1({
  rowsState,
}: {
  rowsState: [
    Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>,
    React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>>>
  ];
}) {
  const { tableApiRef } = useMappings();
  return (
    <ConflictsTable
      rowsState={rowsState}
      tableApiRef={tableApiRef!}
      slots={{ toolbar: CustomToolBar, footer: Footer }}
    />
  );
}

function ThingsTable({
  rowsState,
}: {
  rowsState: [Record<TabsName, IMappedEntry[]>, React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[]>>>];
}) {
  const { getThingsMappedEntriesQuery, tableApiRef } = useMappings();

  return (
    <MappingsTable
      key="things-table"
      getMappedEntriesQuery={getThingsMappedEntriesQuery}
      slots={{ toolbar: CustomToolBar, footer: Footer }}
      tableApiRef={tableApiRef!}
      rowsState={rowsState}
    />
  );
}

function PointsTable({
  rowsState,
}: {
  rowsState: [Record<TabsName, IMappedEntry[]>, React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[]>>>];
}) {
  const { getPointsMappedEntriesQuery, tableApiRef } = useMappings();

  return (
    <MappingsTable
      key="points-table"
      getMappedEntriesQuery={getPointsMappedEntriesQuery}
      slots={{ toolbar: CustomToolBar, footer: Footer }}
      tableApiRef={tableApiRef!}
      rowsState={rowsState}
    />
  );
}

function SpacesTable({
  rowsState,
}: {
  rowsState: [Record<TabsName, IMappedEntry[]>, React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[]>>>];
}) {
  const { getSpacesMappedEntriesQuery, tableApiRef } = useMappings();

  return (
    <MappingsTable
      key="spaces-table"
      getMappedEntriesQuery={getSpacesMappedEntriesQuery}
      slots={{ toolbar: CustomToolBar, footer: Footer }}
      tableApiRef={tableApiRef!}
      rowsState={rowsState}
    />
  );
}

function MiscellaneousTable({
  rowsState,
}: {
  rowsState: [Record<TabsName, IMappedEntry[]>, React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[]>>>];
}) {
  const { getMiscMappedEntriesQuery, tableApiRef } = useMappings();

  return (
    <MappingsTable
      key="misc-table"
      getMappedEntriesQuery={getMiscMappedEntriesQuery}
      slots={{ toolbar: CustomToolBar, footer: Footer }}
      tableApiRef={tableApiRef!}
      rowsState={rowsState}
    />
  );
}

function CustomFooter() {
  const {
    selectAllState,
    tabState,
    getMiscMappedEntriesCountQuery,
    getPointsMappedEntriesCountQuery,
    getSpacesMappedEntriesCountQuery,
    getThingsMappedEntriesCountQuery,
  } = useMappings();
  const { data: miscMappedEntriesCount = 0 } = getMiscMappedEntriesCountQuery();
  const { data: thingsMappedEntriesCount = 0 } = getThingsMappedEntriesCountQuery();
  const { data: pointsMappedEntriesCount = 0 } = getPointsMappedEntriesCountQuery();
  const { data: spacesMappedEntriesCount = 0 } = getSpacesMappedEntriesCountQuery();
  const { data: conflictsCount = 0 } = useGetUpdateTwinCountRequests();

  useEffect(() => {
    switch (tabState[0]) {
      case 'conflicts':
        selectAllState[1]((prev) => ({ ...prev, totalCount: conflictsCount }));
        break;
      case 'miscellaneous':
        selectAllState[1]((prev) => ({ ...prev, totalCount: miscMappedEntriesCount }));
        break;
      case 'points':
        selectAllState[1]((prev) => ({ ...prev, totalCount: pointsMappedEntriesCount }));
        break;
      case 'spaces':
        selectAllState[1]((prev) => ({ ...prev, totalCount: spacesMappedEntriesCount }));
        break;
      case 'things':
        selectAllState[1]((prev) => ({ ...prev, totalCount: thingsMappedEntriesCount }));
        break;
      default:
        selectAllState[1]((prev) => ({ ...prev, totalCount: 0 }));
        break;
    }
  }, [
    selectAllState,
    miscMappedEntriesCount,
    conflictsCount,
    tabState,
    pointsMappedEntriesCount,
    spacesMappedEntriesCount,
    thingsMappedEntriesCount,
  ]);

  return <CustomGridFooter selectAll={selectAllState[0].selectAll} totalCount={selectAllState[0].totalCount} />;
}

function Footer() {
  return (
    <ErrorBoundary FallbackComponent={() => null}>
      <CustomFooter />
    </ErrorBoundary>
  );
}

function CustomToolBar() {
  return (
    <Flex>
      <div>
        <TableTabs />
      </div>
      <StyledToolBarContainer>
        <GridToolbarColumnsButton />
        <GridToolbarFilterButton />
        <GridToolbarDensitySelector />
        <GridToolbarExport />
      </StyledToolBarContainer>
    </Flex>
  );
}

const Flex = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
  justifyContent: 'space-between',
  borderBottom: '0.125rem solid #3b3b3b',
  borderWidth: '0.0625rem',
});

const StyledToolBarContainer = styled(GridToolbarContainer)({ gap: 10, flexWrap: 'wrap', padding: '4px 3px' });

function TableTabs() {
  const {
    tabState,
    getThingsMappedEntriesCountQuery,
    getPointsMappedEntriesCountQuery,
    getSpacesMappedEntriesCountQuery,
    getMiscMappedEntriesCountQuery,
  } = useMappings();
  const [tab, setTab] = tabState;
  const {
    data: thingsCount,
    isLoading: isThingsCountLoading,
    isSuccess: isThingsCountSuccess,
  } = getThingsMappedEntriesCountQuery([Status.Pending]);
  const {
    data: pointsCount,
    isLoading: isPointsCountLoading,
    isSuccess: isPointsCountSuccess,
  } = getPointsMappedEntriesCountQuery([Status.Pending]);
  const {
    data: spacesCount,
    isLoading: isSpacesCountLoading,
    isSuccess: isSpacesCountSuccess,
  } = getSpacesMappedEntriesCountQuery([Status.Pending]);

  const {
    data: miscCount,
    isLoading: isMiscCountLoading,
    isSuccess: isMiscCountSuccess,
  } = getMiscMappedEntriesCountQuery([Status.Pending]);

  const {
    data: conflictsCount,
    isLoading: isConflictsLoading,
    isSuccess: isConflictsCountSuccess,
  } = useGetUpdateTwinCountRequests();

  const renderTabsModel = [
    {
      tabName: 'things',
      numberOfTwins: thingsCount,
      popoverText: 'Assets & Collections',
      isLoading: isThingsCountLoading,
      isSuccess: isThingsCountSuccess,
    },
    {
      tabName: 'points',
      numberOfTwins: pointsCount,
      popoverText: 'Capabilities',
      isLoading: isPointsCountLoading,
      isSuccess: isPointsCountSuccess,
    },
    {
      tabName: 'spaces',
      numberOfTwins: spacesCount,
      popoverText: 'Spatial Resources (including Zones)',
      isLoading: isSpacesCountLoading,
      isSuccess: isSpacesCountSuccess,
    },
    {
      tabName: 'miscellaneous',
      numberOfTwins: miscCount,
      overrideTabName: 'Other twins',
      isLoading: isMiscCountLoading,
      isSuccess: isMiscCountSuccess,
    },
    {
      tabName: 'conflicts',
      numberOfTwins: conflictsCount,
      isLoading: isConflictsLoading,
      isSuccess: isConflictsCountSuccess,
    },
  ];
  return (
    <>
      <WillowTabs value={tab} defaultValue="Things" onTabChange={(value: string | null) => setTab(value as TabsName)}>
        <NoBorderTabList>
          {renderTabsModel.map(({ tabName, numberOfTwins, overrideTabName, ...props }) => (
            <Tab
              key={tabName}
              tabKey={tabName}
              tabName={overrideTabName ?? tabName.charAt(0).toUpperCase() + tabName.slice(1)}
              numberOfTwins={numberOfTwins || 0}
              {...props}
            />
          ))}
        </NoBorderTabList>
      </WillowTabs>
    </>
  );
}
const NoBorderTabList = styled(WillowTabs.List)({ borderBottom: 'none' });

function Tab({
  tabName,
  numberOfTwins,
  popoverText,
  isLoading,
  isSuccess,
  tabKey,
}: {
  tabName: string;
  numberOfTwins: number;
  popoverText?: string;
  isLoading: boolean;
  isSuccess: boolean;
  tabKey: string;
}) {
  const { isLoadingState } = useMappings();
  const [opened, { close, open }] = useDisclosure(false);

  const isDisabled = isLoadingState[0] !== null;

  return (
    <Popover opened={opened} position="top-start" offset={{ mainAxis: -5, alignmentAxis: 24 }} withArrow>
      <Popover.Target>
        <PaddingTab
          value={tabKey}
          suffix={
            isLoading ? <Loader /> : <Badge children={isSuccess ? numberOfTwins : '???'} variant={'bold'} size={'xs'} />
          }
          onMouseEnter={open}
          onMouseLeave={close}
          disabled={isDisabled}
        >
          {tabName}
        </PaddingTab>
      </Popover.Target>
      {popoverText && (
        <Popover.Dropdown>
          <PopoverContent>{popoverText}</PopoverContent>
        </Popover.Dropdown>
      )}
    </Popover>
  );
}
const PopoverContent = styled('div')({
  padding: 8,
  color: 'rgb(198, 198, 198)',
});

const PaddingTab = styled(WillowTabs.Tab)({ padding: '0.6rem !important' });
