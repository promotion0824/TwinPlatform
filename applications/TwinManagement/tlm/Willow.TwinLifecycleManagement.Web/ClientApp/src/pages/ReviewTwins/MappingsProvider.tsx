/* eslint-disable @typescript-eslint/no-unused-vars */
import { createContext, useContext, useState, useEffect } from 'react';
import {
  ApiException,
  ErrorResponse,
  Status,
  IMappedEntry,
  UpdateMappedTwinRequestResponse,
  MappedEntryAllRequest,
  IMappedEntryAllRequest,
} from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import useGetMappedEntries, { IGetMappedEntries } from './hooks/useGetMappedEntries';
import usePutMappedEntry, { IPutMappedEntry } from './hooks/usePutMappedEntry';
import useGetMappedEntriesCount, { IGetMappedEntriesCount } from './hooks/useGetMappedEntriesCount';
import { Alert, AlertProps, Snackbar } from '@mui/material';
import { GridRowId, useGridApiRef, GridRowModelUpdate } from '@mui/x-data-grid-pro';
import useApi from '../../hooks/useApi';
import { useQueryClient, useMutation, UseMutationResult } from 'react-query';
import useMultipleSearchParams from '../../hooks/useMultipleSearchParams';
import useHandleTabsQueryParams from './hooks/useHandleTabsQueryParams';
import { useSnackbar } from '../../providers/SnackbarProvider/SnackbarProvider';
import useGetTwinsLookup, { UseGetTwinsLookup } from './hooks/useTwinsLookup';

type MappingsContextType = {
  getThingsMappedEntriesQuery: IGetMappedEntries;
  getPointsMappedEntriesQuery: IGetMappedEntries;
  getSpacesMappedEntriesQuery: IGetMappedEntries;
  getMiscMappedEntriesQuery: IGetMappedEntries;
  putMappedEntryMutate: IPutMappedEntry;
  tableApiRef: ReturnType<typeof useGridApiRef>;
  selectedRowsState: [GridRowId[], React.Dispatch<React.SetStateAction<GridRowId[]>>];
  handleStatusChange: (status: Status) => void;
  handleDeleteAll: () => void;
  tabState: [TabsName | undefined, React.Dispatch<React.SetStateAction<TabsName | undefined>>];
  getThingsMappedEntriesCountQuery: (statuses?: Status[]) => IGetMappedEntriesCount;
  getPointsMappedEntriesCountQuery: (statuses?: Status[]) => IGetMappedEntriesCount;
  getSpacesMappedEntriesCountQuery: (statuses?: Status[]) => IGetMappedEntriesCount;
  getMiscMappedEntriesCountQuery: (statuses?: Status[]) => IGetMappedEntriesCount;
  getAllMappedEntriesCountQuery: () => IGetMappedEntriesCount;
  cellCoordinateState: [CellCoordinate, React.Dispatch<React.SetStateAction<CellCoordinate>>];
  isLoadingState: [Status | 'deleting' | null, React.Dispatch<React.SetStateAction<Status | 'deleting' | null>>];
  changeMappedEntriesStatusMutate: UseMutationResult<any, any, any, any>;
  syncRowsState: [
    Record<TabsName, GridRowModelUpdate[]>,
    React.Dispatch<React.SetStateAction<Record<TabsName, GridRowModelUpdate[]>>>
  ];
  handleDeleteBulk: () => void;
  rowsState: [
    Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>,
    React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>>>
  ];
  connectorIdState: [string | null, (value: string | null) => void];
  buildingIdsState: [string[], React.Dispatch<React.SetStateAction<string[]>>];
  twinsLookup: UseGetTwinsLookup;
  disabledBuildingFilter: boolean;
  disabledConnectorFilter: boolean;
  selectAllState: [SelectAllType, React.Dispatch<React.SetStateAction<SelectAllType>>];
};
type SelectAllType = { selectAll: boolean; totalCount: number };
type CellCoordinate = { rowIndex: number; colIndex: number };

const MappingsContext = createContext<MappingsContextType | undefined>(undefined);

export function useMappings() {
  const context = useContext(MappingsContext);
  if (context == null) {
    throw new Error('useMappings must be used within a MappingsProvider');
  }

  return context;
}

export type TabsName = 'things' | 'points' | 'spaces' | 'miscellaneous' | 'conflicts';

// hard-coded prefixes that Mapped uses for their id. Used to define Things, Spaces and Points.
const MAPPED_THINGS_PREFIXES = ['THG'];
const MAPPED_POINTS_PREFIXES = ['PNT'];
const MAPPED_SPACES_PREFIXES = ['SITE', 'FLR', 'BLDG', 'ZONE', 'SPC'];

type MappedEntryRequestParam = Omit<IMappedEntryAllRequest, 'buildingIds' | 'connectorId'>;
type MappedEntryRequestsParam = Record<string, MappedEntryRequestParam>;
const mappedEntryRequestsParam: MappedEntryRequestsParam = {
  things: { prefixToMatchId: MAPPED_THINGS_PREFIXES, excludePrefixes: false, statuses: [Status.Pending] },
  points: { prefixToMatchId: MAPPED_POINTS_PREFIXES, excludePrefixes: false, statuses: [Status.Pending] },
  spaces: { prefixToMatchId: MAPPED_SPACES_PREFIXES, excludePrefixes: false, statuses: [Status.Pending] },
  miscellaneous: {
    prefixToMatchId: [...MAPPED_THINGS_PREFIXES, ...MAPPED_POINTS_PREFIXES, ...MAPPED_SPACES_PREFIXES],
    excludePrefixes: true,
    statuses: [Status.Pending],
  },
  conflicts: { prefixToMatchId: [], excludePrefixes: false, statuses: [Status.Pending] },
  default: { prefixToMatchId: [], excludePrefixes: false, statuses: [Status.Pending] },
};

export default function MappingsProvider({ children }: { children: React.ReactNode }) {
  const api = useApi();
  const queryClient = useQueryClient();
  const tableApiRef = useGridApiRef();

  const tabState = useState<TabsName>();
  // states used for error handling
  const [openPopUp, setOpenPopUp] = useState(true);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  useHandleTabsQueryParams(tabState);

  const mappedEntryRequestParam = mappedEntryRequestsParam[tabState[0] || 'default'];
  const { prefixToMatchId, excludePrefixes } = mappedEntryRequestParam;
  const connectorIdState = useState<string | null>(null);
  const buildingIdsState = useState<string[]>([]);
  const disabledBuildingFilter = tabState[0] === 'miscellaneous' || tabState[0] === 'conflicts';
  const disabledConnectorFilter =
    tabState[0] === 'spaces' || tabState[0] === 'miscellaneous' || tabState[0] === 'conflicts';

  const getThingsMappedEntriesQuery = useGetMappedEntries(
    prefixToMatchId,
    excludePrefixes,
    disabledBuildingFilter ? undefined : buildingIdsState[0],
    disabledConnectorFilter ? undefined : connectorIdState[0],
    {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
      enabled: tabState[0] === 'things',
    }
  );

  const getPointsMappedEntriesQuery = useGetMappedEntries(
    prefixToMatchId,
    excludePrefixes,
    disabledBuildingFilter ? undefined : buildingIdsState[0],
    disabledConnectorFilter ? undefined : connectorIdState[0],
    {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
      enabled: tabState[0] === 'points',
    }
  );

  const getSpacesMappedEntriesQuery = useGetMappedEntries(
    prefixToMatchId,
    excludePrefixes,
    disabledBuildingFilter ? undefined : buildingIdsState[0],
    disabledConnectorFilter ? undefined : connectorIdState[0],
    {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
      enabled: tabState[0] === 'spaces',
    }
  );

  const getMiscMappedEntriesQuery = useGetMappedEntries(
    prefixToMatchId,
    excludePrefixes,
    disabledBuildingFilter ? undefined : buildingIdsState[0],
    disabledConnectorFilter ? undefined : connectorIdState[0],
    {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
      enabled: tabState[0] === 'miscellaneous',
    }
  );

  const getThingsMappedEntriesCountQuery = (statuses?: Status[]) => {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useGetMappedEntriesCount(statuses, MAPPED_THINGS_PREFIXES, false, {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
    });
  };

  const getPointsMappedEntriesCountQuery = (statuses?: Status[]) => {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useGetMappedEntriesCount(statuses, MAPPED_POINTS_PREFIXES, false, {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
    });
  };

  const getSpacesMappedEntriesCountQuery = (statuses?: Status[]) => {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useGetMappedEntriesCount(statuses, MAPPED_SPACES_PREFIXES, false, {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
    });
  };

  const getMiscMappedEntriesCountQuery = (statuses?: Status[]) => {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useGetMappedEntriesCount(
      statuses,
      [...MAPPED_THINGS_PREFIXES, ...MAPPED_POINTS_PREFIXES, ...MAPPED_SPACES_PREFIXES],
      true,
      {
        onError: (error) => {
          setErrorMessage(error);
          setOpenPopUp(true);
        },
      }
    );
  };

  const getAllMappedEntriesCountQuery = () => {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useGetMappedEntriesCount([], [], false, {
      onError: (error) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
    });
  };

  const putMappedEntryMutate = usePutMappedEntry({
    onError: (error) => {
      setSnackbar({ children: 'Error saving', severity: 'error' });
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries('mapped-entities-count');
      syncRowsState[1]((prev) => ({ ...prev, [tabState[0]!]: [...prev[tabState[0]!], data] }));
      setSnackbar({ children: 'Successfully saved', severity: 'success' });
    },
  });

  const [snackbar, setSnackbar] = useState<Pick<AlertProps, 'children' | 'severity'> | null>(null);

  const handleCloseSnackbar = () => setSnackbar(null);

  const selectedRowsState = useState<GridRowId[]>([]);

  const isLoadingState = useState<Status | 'deleting' | null>(null);

  const changeMappedEntriesStatusMutate = useMutation(
    (params: { mappedIds: string[]; status: Status }) => api.changeMappedEntriesStatus(params.status, params.mappedIds),
    {}
  );
  const { mutateAsync: changeMappedEntriesStatusMutateAsync } = changeMappedEntriesStatusMutate;

  const changeAllMappedEntriesStatusMutate = useMutation(
    ({ request, status }: { request: MappedEntryAllRequest; status: Status }) => api.updateAllstatus(status, request),
    {}
  );
  const { mutateAsync: changeAllMappedEntriesStatusMutateAsync } = changeAllMappedEntriesStatusMutate;

  const handleStatusChange = async (status: Status) => {
    try {
      if (selectedRowsState[0].length === 0 && !selectAllState[0].selectAll) {
        return;
      }

      let newRows = Array.from(tableApiRef.current.getSelectedRows().values())
        .filter((row) => row)
        .filter((row) => row.status !== Status.Created) // rows  with "Created" status should not be changed
        .map((row) => ({ ...row, status: status }));
      // @ts-ignore
      let newRowsIds = newRows.map((row) => row.mappedId);
      isLoadingState[1](status);

      let params = { mappedIds: newRowsIds, status: status };

      mappedEntryAllRequest.statuses = [Status.Pending, Status.Ignore, Status.Approved]; // only update rows that is not "Created" status

      const numOfSuccessfullUpdateRecords = selectAllState[0].selectAll
        ? await changeAllMappedEntriesStatusMutateAsync({ request: mappedEntryAllRequest, status })
        : await changeMappedEntriesStatusMutateAsync(params);

      tableApiRef.current.updateRows(newRows);
      snackbar1.show(`${numOfSuccessfullUpdateRecords} records ${status === Status.Approved ? 'approved' : 'ignored'}`);

      if (selectAllState[0].selectAll) {
        selectAllState[1]({ selectAll: false, totalCount: 0 });
      }

      syncRowsState[1]((prev) => ({ ...prev, [tabState[0]!]: [...prev[tabState[0]!], ...newRows] })); // store updated row changes so we can sync with MUI grid whenever new data is fetched via infinite scrolling

      // clear selected rows
      selectedRowsState[1]([]);
      tableApiRef.current.setRowSelectionModel([]);

      queryClient.invalidateQueries('mapped-entities-count');
      queryClient.invalidateQueries('get-ApproveAndAccept-filters-dropdown');
    } finally {
      isLoadingState[1](null);
    }
  };

  const deleteAllMutate = useMutation(({ request }: { request: MappedEntryAllRequest }) => api.deleteAll(request), {});
  const { mutateAsync: deleteAllMutateAsync } = deleteAllMutate;

  // Used for "delete all" button, where its display when isDevmode is on.
  // Delete all approve and accept records
  const handleDeleteAll = async () => {
    deleteAllMutateAsync({ request: new MappedEntryAllRequest({}) }).then(() => {
      snackbar1.show(`Successfully deleted all records`);
      // get all fetched rows across all mapped entries tabs UI table, and remove the rows that are deleted,  so UI will be in-sync with the server-side changes.
      const deletedRowsGridRowModelUpdate = Object.keys(rowsState[0]).reduce((acc, key) => {
        if (key === 'conflicts') return acc;
        acc[key as TabsName] = rowsState[0][key as TabsName].map((row: IMappedEntry) => {
          return { mappedId: row.mappedId, _action: 'delete' };
        });
        return acc;
      }, {} as Record<TabsName, GridRowModelUpdate[]>);

      syncRowsState[1](deletedRowsGridRowModelUpdate);

      // clear selected rows
      selectedRowsState[1]([]);
      tableApiRef.current.setRowSelectionModel([]);

      queryClient.invalidateQueries();
    });
  };

  const snackbar1 = useSnackbar();

  const deleteBulkMutate = useMutation(({ ids }: { ids: string[] }) => api.deleteBulk(ids), {});
  const { mutateAsync: deleteBulkMutateAsync } = deleteBulkMutate;

  const [urlParams] = useMultipleSearchParams([{ name: 'devMode', type: 'string' }]);

  const devModesUrlParam = (urlParams?.devMode || '') as string;
  const isDevMode = devModesUrlParam.toLowerCase() === 'true';

  const mappedEntryAllRequest = getMappedEntryAllRequest(
    mappedEntryRequestParam,
    buildingIdsState[0],
    connectorIdState[0]!
  );

  const handleDeleteBulk = async () => {
    isLoadingState[1]('deleting');
    try {
      let pendingRows = Array.from(tableApiRef.current.getSelectedRows().values()).filter(
        (row) => row.status === Status.Pending || isDevMode
      ); // only delete rows with "Pending" status

      let pendingRowIds = pendingRows.map((row) => row.mappedId);

      const numOfSuccessfulDeleteRecords = selectAllState[0].selectAll
        ? await deleteAllMutateAsync({ request: mappedEntryAllRequest })
        : await deleteBulkMutateAsync({ ids: pendingRowIds });

      const gridRowModelUpdate = pendingRowIds.map((id) => ({ mappedId: id, _action: 'delete' } as GridRowModelUpdate));

      if (selectAllState[0].selectAll) {
        selectAllState[1]({ selectAll: false, totalCount: 0 });
        queryClient.invalidateQueries('mapped-entities');
      }
      syncRowsState[1]((prev) => ({ ...prev, [tabState[0]!]: [...prev[tabState[0]!], ...gridRowModelUpdate] }));

      // clear selected rows
      tableApiRef.current.setRowSelectionModel([]);
      selectedRowsState[1]([]);

      queryClient.invalidateQueries('mapped-entities-count');
      queryClient.invalidateQueries('get-ApproveAndAccept-filters-dropdown');
      snackbar1.show(`${numOfSuccessfulDeleteRecords} records deleted successfully`);
      tableApiRef.current.updateRows(gridRowModelUpdate);
    } finally {
      isLoadingState[1](null);
    }
  };

  // MUI Datagrid's cell focus positions
  const cellCoordinateState = useState<CellCoordinate>({
    rowIndex: 0,
    colIndex: 0,
  });

  // track the rows that've been updated, so we can sync MUI grid when data is fetched.
  const syncRowsState = useState<Record<TabsName, GridRowModelUpdate[]>>({
    things: [],
    points: [],
    spaces: [],
    miscellaneous: [],
    conflicts: [],
  });

  const defaultRows = {
    things: [],
    points: [],
    spaces: [],
    miscellaneous: [],
    conflicts: [],
  };

  // Todo: fix janky way of handling persistent rows between tabs. Maybe use react-query's infinite query
  const rowsState = useState<Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>>(defaultRows);

  // Clean slate when filters change
  useEffect(
    () => {
      rowsState[1](defaultRows);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [connectorIdState[0], buildingIdsState[0]]
  );

  const twinsLookup = useGetTwinsLookup();

  const selectAllState = useState<SelectAllType>({ selectAll: false, totalCount: 0 });

  return (
    <MappingsContext.Provider
      value={{
        getThingsMappedEntriesQuery,
        getPointsMappedEntriesQuery,
        getSpacesMappedEntriesQuery,
        getMiscMappedEntriesQuery,
        putMappedEntryMutate,
        tableApiRef,
        selectedRowsState,
        handleStatusChange,
        handleDeleteAll,
        tabState,
        getThingsMappedEntriesCountQuery,
        getPointsMappedEntriesCountQuery,
        getSpacesMappedEntriesCountQuery,
        getMiscMappedEntriesCountQuery,
        getAllMappedEntriesCountQuery,
        cellCoordinateState,
        isLoadingState,
        changeMappedEntriesStatusMutate,
        syncRowsState,
        handleDeleteBulk,
        rowsState,
        connectorIdState,
        buildingIdsState,
        twinsLookup,
        disabledBuildingFilter,
        disabledConnectorFilter,
        selectAllState,
      }}
    >
      {
        // todo: remove when global error handling is implemented
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      }
      {children}

      {!!snackbar && (
        <Snackbar
          open
          anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
          onClose={handleCloseSnackbar}
          autoHideDuration={6000}
        >
          <Alert {...snackbar} onClose={handleCloseSnackbar} variant="filled" />
        </Snackbar>
      )}
    </MappingsContext.Provider>
  );
}

function getMappedEntryAllRequest(parameters: MappedEntryRequestParam, buildingIds: string[], connectorId: string) {
  const request = new MappedEntryAllRequest();
  const { prefixToMatchId, excludePrefixes, statuses } = parameters;
  request.prefixToMatchId = prefixToMatchId;
  request.excludePrefixes = excludePrefixes;
  request.buildingIds = buildingIds;
  request.connectorId = connectorId;
  request.statuses = [Status.Pending];

  return request;
}
