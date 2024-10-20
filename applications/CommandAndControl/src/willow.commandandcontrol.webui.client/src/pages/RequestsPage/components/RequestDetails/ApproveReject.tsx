import React, { useState } from "react";
import { ActionsContainer } from "../../../../components/Styled/ActionsContainer"
import { RequestedCommandResponseDto, RequestedCommandStatus } from "../../../../services/Clients";
import { ActionButton } from "./ActionButton";
import { useRequests } from "../../RequestsProvider";
import { GridCellParams } from "@willowinc/ui";

export const ApproveReject: React.FC<ApproveRejectProps> = ({ params }) => {

  const {row: request} = params;
  const { requestedApproveRejectState, statusUpdating } = useRequests();

  const handleApprove = () => setStatus(RequestedCommandStatus.Approved);
  const handleReject = () => setStatus(RequestedCommandStatus.Rejected);

  const setStatus = (status: RequestedCommandStatus) => {
    let newObj = { ...requestedApproveRejectState[0] };
    newObj[request.id!] = status;
    requestedApproveRejectState[1](newObj);
  }

  const status = requestedApproveRejectState[0][request.id!] ?? request.status;
  const locked = request.status !== RequestedCommandStatus.Pending;

  return (

    <ActionsContainer>
      <ActionButton
        onClick={handleApprove}
        value={status === RequestedCommandStatus.Approved ? "Approved" : "Approve"}
        loading={statusUpdating}
        locked={locked}
        selected={status === RequestedCommandStatus.Approved}
      />
      <ActionButton
        onClick={handleReject}
        value={status === RequestedCommandStatus.Rejected ? "Rejected" : "Reject"}
        loading={statusUpdating}
        locked={locked}
        selected={status === RequestedCommandStatus.Rejected}
      />
    </ActionsContainer>
  );
}

export interface ApproveRejectProps {
  params: GridCellParams<RequestedCommandResponseDto>;
  //request: RequestedCommandResponseDto;
}
