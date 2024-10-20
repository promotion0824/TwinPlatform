import { useMemo, useState, useEffect } from "react";
import { GridCellParams, DataGridProProps } from "@mui/x-data-grid-pro";
import { GridColDef, GridEventLookup, GridRowParams } from "@willowinc/ui";
import { useRequests, RequestsType } from "../RequestsProvider";
import TwinChip from "../../../components/TwinChip/TwinChip";
import { IGetRequestedCommands } from "../hooks/useGetRequestedCommands";
import { formatDate } from "../../../utils/formatDate";
import { GetConflictingCommandPresentValuesRequestDto, IConflictingCommandsResponseDto, SortSpecificationDto } from "../../../services/Clients";
import { useAppContext } from "../../../providers/AppContextProvider";
import { NoRowsOverlay } from "../../../components/Table/NoRowsOverlay";
import { StyledDataGrid } from "../../../components/Styled/StyledDataGrid";
import { useNavigate } from "react-router-dom";
import usePostResolvedCommandStatus from "../../CommandsPage/hooks/usePostResolvedCommandStatus";
import usePostRequestedCommandPresentValue from "../hooks/usePostRequestedCommandPresentValue";

export default function RequestsTable({
  type,
  requestedCommandsQuery,
}: {
  type: RequestsType;
  requestedCommandsQuery: IGetRequestedCommands;
}) {

  const navigate = useNavigate();
  const { selectedRowState, apiRef, } = useRequests();
  const { selectedSite } = useAppContext();
  const { mutate: updatePresentValue } = usePostResolvedCommandStatus();

  const columns: any = useMemo(() => {
    let cols: GridColDef<IConflictingCommandsResponseDto>[] = [
      {
        field: "twinId",
        headerName: "Capability",
        flex: 0.2,
      },
      {
        field: "isCapabilityOf",
        headerName: "Asset",
        flex: 0.2,
        renderCell: (params: any) => (
          <TwinChip
            type="asset"
            value={params.value ?? params.row.isHostedBy ?? "Unknown"}
          />
        ),
      },
    ];

    if (selectedSite === "allSites") {
      cols.push(
        {
          field: "location",
          headerName: "Location",
          flex: 0.2,
          renderCell: (params: any) => (
            <TwinChip type="site" value={params.value ?? "Unknown"} />
          ),
        });
    }

    cols = [...cols,
    {
      field: "presentValue",
      headerName: "Present Value",
      flex: 0.2,
      sortable: false,
      renderCell: (params: any) => !!params.value || params.value === 0 ? `${params.value}${params.row.unit}` : "-",
    },
    {
      field: "commands",
      headerName: "Commands",
      flex: 0.2,
    },
    {
      field: "approvedCommands",
      headerName: "Approved",
      flex: 0.2,
    },
    {
      field: "receivedDate",
      headerName: "Received",
      renderCell: (params: GridCellParams<IConflictingCommandsResponseDto>) => formatDate(params.value as Date),
      flex: 0.2,
    },
    ];
    return cols;
  },
    [selectedSite]
  );

  const { query, sortState } = requestedCommandsQuery;
  const { data, isLoading, isFetching, isError, fetchNextPage, isFetchingNextPage } = query;

  const totalRows = data?.pages[0]?.total || 0;
  const [rows, setRows] = useState<IConflictingCommandsResponseDto[]>([]);
  const [externalIdsToFetch, setExternalIdsToFetch] = useState<string[]>([]);

  const handleOnRowsScrollEnd: DataGridProProps["onRowsScrollEnd"] = () => {
    if (!isFetching && !isFetchingNextPage && rows.length < totalRows && rows.length !== 0) {
      fetchNextPage();
    }
  };

  const { data: presentValueData, isLoading: isPresentValueLoading, isError: isPresentValueError, } = usePostRequestedCommandPresentValue(
    new GetConflictingCommandPresentValuesRequestDto({ externalIds: externalIdsToFetch }
    ));

  useEffect(() => {
    if (data) {
      const items = data.pages.flatMap(a => a.items ?? []) ?? [];

      const existingPresentValues = Object.fromEntries(rows.map(x => [x.externalId, x.presentValue]));
      setRows(items.map(item => ({ ...item, presentValue: item.externalId ? existingPresentValues[item.externalId] : null })));

      const itemsWithoutPresentValue = items.filter(item => !item.presentValue);
      const externalIds = itemsWithoutPresentValue.map(item => item.externalId).filter((id): id is string => id !== undefined);

      if (externalIds.length > 0) {
        setExternalIdsToFetch(externalIds);
      } else {
        setExternalIdsToFetch([]);
      }
    }
  }, [data?.pages]);

  useEffect(() => {
    if (presentValueData) {
      const updatedRows = rows.map(row => ({
        ...row,
        presentValue: presentValueData.presentValues?.[row.externalId ?? ''] ?? row.presentValue
      }));
      setRows(updatedRows);
    }
  }, [presentValueData]);

  return (
    <StyledDataGrid
      apiRef={apiRef}
      rows={isLoading || isError ? [] : rows}
      getRowId={(row: IConflictingCommandsResponseDto) => row.key!}
      columns={columns}
      loading={isLoading}
      rowCount={totalRows}
      hideFooterPagination
      disableColumnReorder
      hideFooterSelectedRowCount
      onRowClick={(newRowSelectionModel: GridRowParams<IConflictingCommandsResponseDto>) => {
        selectedRowState[1](newRowSelectionModel.row);
        navigate(`/requests/${newRowSelectionModel.row.connectorId}/${newRowSelectionModel.row.twinId}`);
      }}
      initialState={{
        sorting: { sortModel: [{ field: "receivedDate", sort: "desc" }] },
      }}
      onRowsScrollEnd={handleOnRowsScrollEnd}
      onSortModelChange={(model) => {
        sortState[1](model.map
          (m => new SortSpecificationDto({
            field: m.field,
            sort: m.sort?.toString(),
          })));
      }}
      slots={{
        noRowsOverlay: NoRowsOverlay,
      }}
      slotProps={{
        noRowsOverlay: { isError },
      }}
    />
  );
}
