import { DataGrid, DataGridProps, GridColDef, GridRenderCellParams, GridRowParams } from "@willowinc/ui";
import { useEffect, useMemo, useState } from "react";

import { NoRowsOverlay } from "../../../components/Table/NoRowsOverlay";
import TwinChip from "../../../components/TwinChip/TwinChip";
import { useAppContext } from "../../../providers/AppContextProvider";
import { ActivityLogsResponseDto, ActivityType, IActivityLogsResponseDto, SortSpecificationDto, User } from "../../../services/Clients";
import { formatDate } from "../../../utils/formatDate";
import { useActivityLogs } from "../ActivityLogsProvider";
import { ActivityLogMessage } from "./ActivityLogMessage";
import { ActivityLogRole } from "./ActivityLogRole";
import { StyledDataGrid } from "../../../components/Styled/StyledDataGrid";
import { Link } from "react-router-dom";

export const ActivityLogsTable = () => {

  const { getActivityLogsQuery, modalState, selectedRowState, apiRef, compact } = useActivityLogs();
  const { selectedSite } = useAppContext();

  const columns: GridColDef<ActivityLogsResponseDto>[] = useMemo(
    () => {
      const cols: GridColDef<ActivityLogsResponseDto>[] = [
        {
          field: "timestamp",
          headerName: "Time",
          renderCell: (params: GridRenderCellParams<ActivityLogsResponseDto, string>) =>
            formatDate(
              params.row.timestamp
            ),
          flex: 0.2,
        },
        {
          field: "type",
          headerName: "Activity",
          renderCell: (params: GridRenderCellParams<ActivityLogsResponseDto, ActivityType>) => <ActivityLogMessage activityLog={params.row} />,
          flex: 0.8,
        },
        {
          field: "updatedBy",
          headerName: "Role",
          renderCell: (params: GridRenderCellParams<ActivityLogsResponseDto, User>) => <ActivityLogRole activityLog={params.row} />,
          flex: 0.4,
        }];

      if (compact) return cols;

      cols.push({
        field: "commandName",
        headerName: "Command",
        flex: 0.8,
        renderCell: (params: GridRenderCellParams<ActivityLogsResponseDto, string>) =>
          !!params.row.resolvedCommandId ? <Link to={`/commands/${params.row.resolvedCommandId}`}>{params.value ?? "Unknown"}</Link> : <Link to={`/requests/${params.row.connectorId}/${params.row.twinId}`}>{params.value ?? "Unknown"}</Link>
      });
      cols.push({
        field: "isCapabilityOf",
        headerName: "Asset",
        flex: 0.4,
        renderCell: (params: any) => (
          <TwinChip type="asset" value={params.value ?? params.row.isHostedBy ?? "Unknown"}
          />
        ),
      });

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

      return cols;
    },
    [selectedSite, compact]
  );

  const { query, sortState } = getActivityLogsQuery;
  const { data, isLoading, isFetching, isError, fetchNextPage } = query;

  const totalRows = data?.pages[0]?.total || 0;
  const [rows, setRows] = useState<ActivityLogsResponseDto[]>([]);

  const handleOnRowsScrollEnd: DataGridProps["onRowsScrollEnd"] = () => {
    if (!isFetching && rows.length < totalRows && rows.length !== 0) {
      fetchNextPage();
    }
  };

  useEffect(() => {
    setRows(() => data?.pages.flatMap(d => d.items ?? []) ?? []);
  }, [data, isLoading]);

  return (
    <StyledDataGrid
      apiRef={apiRef}
      rows={isLoading || isError ? [] : rows}
      getRowId={(row: IActivityLogsResponseDto) => row.id!}
      columns={columns as any}
      loading={isLoading}
      rowCount={totalRows}
      hideFooterPagination
      disableColumnReorder
      hideFooterSelectedRowCount
      onRowClick={(params: GridRowParams<ActivityLogsResponseDto>) => {
        selectedRowState[1](params.row);
        modalState[1].open();
      }}
      initialState={{
        sorting: { sortModel: [{ field: "timestamp", sort: "desc" }] },
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
};
