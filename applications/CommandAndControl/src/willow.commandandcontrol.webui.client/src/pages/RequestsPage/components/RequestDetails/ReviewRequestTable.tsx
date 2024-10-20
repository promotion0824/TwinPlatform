import React, { useMemo } from "react";
import { StyledDataGrid } from "../../../../components/Styled/StyledDataGrid";
import useAuthorization from "../../../../hooks/useAuthorization";
import { formatDate } from "../../../../utils/formatDate";
import { RequestsType, useRequests } from "../../RequestsProvider";
import { ApproveReject } from "./ApproveReject";

export const ReviewRequestTable: React.FC<ReviewRequestsTableProps> = ({ requestsType }) => {

  const { reviewRequestTableApiRef, selectedRowState, requestDetailsLoading, requestedApproveRejectState } = useRequests();
  const { hasCanApproveExecutePermission } = useAuthorization();

  const columns: any = useMemo(
    () => [
      {
        field: "ruleId",
        headerName: "Rule",
        flex: 1,
      },
      {
        field: "commandName",
        headerName: "Command",
        flex: 1,
      },
      {
        field: "type",
        headerName: "Type",
        flex: 0.5,
      },
      {
        field: "value",
        headerName: "Target Value",
        flex: 0.6,
        valueGetter: (params: any) => `${params.value}${params.row.unit}`,
      },
      {
        field: "startTime",
        headerName: "Start Time",
        renderCell: (params: any) => formatDate(params.row.startTime),
        flex: 0.4,
      },
      {
        field: "endTime",
        headerName: "End Time",
        renderCell: (params: any) => formatDate(params.row.endTime),
        flex: 0.4,
      },
      hasCanApproveExecutePermission &&
      {
        field: "actions",
        headerName: "Actions",
        type: "actions",
        flex: 0.8,
        sortable: false,
        renderCell: (params: any) => {
          return <ApproveReject params={params} />;
        },
      },
    ],
    [requestedApproveRejectState,hasCanApproveExecutePermission]
  );

  return (
    <StyledDataGrid
      apiRef={reviewRequestTableApiRef}
      rows={selectedRowState[0]?.requests ?? []}
      getRowId={(row: any) => row.id!}
      loading={requestDetailsLoading}
      columns={columns}
      getRowHeight={() => "auto"}
      disableColumnReorder
      hideFooter
      disableRowSelectionOnClick
      initialState={{
        sorting: { sortModel: [{ field: "startTime", sort: "desc" }] }
      }}
    />
  );
}

export interface ReviewRequestsTableProps {
  requestsType: RequestsType;
}
