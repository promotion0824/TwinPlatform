import styled from "@emotion/styled";
import { DataGridProProps } from "@mui/x-data-grid-pro";
import { GridColDef } from "@willowinc/ui";
import { useEffect, useMemo, useState } from "react";

import { useNavigate } from "react-router-dom";
import { ActionsContainer } from "../../../../components/Styled/ActionsContainer";
import { StyledDataGrid } from "../../../../components/Styled/StyledDataGrid";
import { NoRowsOverlay } from "../../../../components/Table/NoRowsOverlay";
import TwinChip from "../../../../components/TwinChip/TwinChip";
import useAuthorization from "../../../../hooks/useAuthorization";
import { useAppContext } from "../../../../providers/AppContextProvider";
import { IResolvedCommandResponseDto, ResolvedCommandAction, ResolvedCommandResponseDto, SortSpecificationDto } from "../../../../services/Clients";
import { formatDate } from "../../../../utils/formatDate";
import { useCommands } from "../../CommandsProvider";
import { IGetResolvedCommands } from "../../hooks/useGetResolvedCommands";
import ActionButton from "./ActionButton";
import CommandStatus from "./CommandStatus";

export default function CommandsTable({
  resolvedCommandQuery,
  showActions,
}: {
  resolvedCommandQuery: IGetResolvedCommands;
  showActions?: boolean;
}) {
  const navigate = useNavigate();
  const { apiRef, handleVerifyAction } = useCommands();
  const { selectedSite } = useAppContext();
  const { hasCanApproveExecutePermission } = useAuthorization();

  const columns: GridColDef<IResolvedCommandResponseDto>[] = useMemo(() => {
    let cols: GridColDef<IResolvedCommandResponseDto>[] =
      [
        {
          field: "twinId",
          headerName: "Capability",
          flex: 1,
        },
        {
          field: "isCapabilityOf",
          headerName: "Asset",
          flex: 1,
          renderCell: (params) => (
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
          flex: 1,
          renderCell: (params) => (
            <TwinChip type="site" value={params.value ?? "Unknown"} />
          ),
        });
    }

    cols = [...cols,
    {
      field: "commandName",
      headerName: "Command",
      flex: 1,

    },
    {
      field: "presentValue",
      headerName: "Present Value",
      flex: 0.5,
      sortable: false,
      renderCell: (params: any) => !!params.value || params.value === 0 ? `${params.value}${params.row.unit}` : "-",
    },
    {
      field: "value",
      headerName: "Target Value",
      flex: 0.5,
      renderCell: (params: any) => !!params.value || params.value === 0 ? `${params.value}${params.row.unit}` : "-",
    },
    {
      field: "createdDate",
      headerName: "Created Date",
      renderCell: (params) => formatDate(params.row.createdDate),
      flex: 1,
    },
    {
      field: "startTime",
      headerName: "Start Time",
      renderCell: (params) => formatDate(params.row.startTime),
      flex: 1,
    },

    {
      field: "endTime",
      renderCell: (params) => formatDate(params.row.endTime),
      headerName: "End Time",
      flex: 1,
    },
    {
      field: "status",
      headerName: "Status",
      flex: 0.8,
      renderCell: (params) => <CommandStatus value={params.row.status!} />,
    },
    {
      field: "statusUpdatedBy",
      headerName: "Actioned By",
      flex: 1,
      renderCell: (params: any) => {
        return params.value.name;
      },
    },
    ];

    if (!!showActions && hasCanApproveExecutePermission) {
      cols.push({
        field: "actions",
        headerName: "Actions",
        flex: 1.5,
        type: "actions",
        cellClassName: "actions",
        renderCell: (params) => (
          <ActionsContainer>
            {params.value?.map((val: ResolvedCommandAction) => (
              <div key={`${params.row.id}${val}`}>
                <ActionButton
                  value={val}
                  onClick={(e) => {
                    e.stopPropagation();
                    handleVerifyAction(params.row.id!, val);
                  }}
                />
              </div>
            ))}
          </ActionsContainer>
        ),
      });
    }
    return cols;
  },
    [showActions, selectedSite, hasCanApproveExecutePermission]
  );

  const { query, sortState } = resolvedCommandQuery;
  const { data, isLoading, isFetching, isError, fetchNextPage } = query;

  const totalRows = data?.pages[0]?.total || 0;
  const [rows, setRows] = useState<ResolvedCommandResponseDto[]>([]);

  const handleOnRowsScrollEnd: DataGridProProps["onRowsScrollEnd"] = () => {
    if (!isFetching && rows.length < totalRows && rows.length !== 0) {
      fetchNextPage();
    }
  };

  useEffect(() => {
    setRows(data?.pages.flatMap(a => a.items ?? []) ?? []);
  }, [data?.pages]);

  return (
    <>
      <StyledDataGrid
        className="commands-grid"
        apiRef={apiRef}
        rows={isLoading || isError ? [] : rows}
        getRowId={(row: IResolvedCommandResponseDto) => row.id!}
        columns={columns}
        loading={isLoading}
        rowCount={0}
        hideFooterPagination
        disableColumnReorder
        getRowHeight={() => "auto"}
        onRowSelectionModelChange={(newRowSelectionModel: any) => {
          navigate("/commands/" + newRowSelectionModel[0]);
        }}
        initialState={{
          sorting: { sortModel: [{ field: "createdDate", sort: "desc" }] },
        }}
        hideFooterSelectedRowCount
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
          noRowsOverlay: { isError }
        }}
      />
    </>
  );
}
