import { Button, PanelGroup, Panel, ButtonGroup } from "@willowinc/ui";
import styled from "@emotion/styled";
import { ReviewRequestTable } from "./ReviewRequestTable";
import { useRequests } from "../../RequestsProvider";
import TwinChip from "../../../../components/TwinChip/TwinChip";
import { IConflictingCommandsResponseDto } from "../../../../services/Clients";
import React from "react";
import { HeightPanelContent } from "../../../../components/Styled/HeightPanelContent";

export const RequestDetails = () => {
  const { selectedRowState, handleReviewRequestResolve, handleReviewRequestCancel, requestedApproveRejectState, statusUpdating } = useRequests();

  const request = selectedRowState[0];

  if (!request) return null;

  return (
    <PanelGroup units="percentages">
      <Panel collapsible defaultSize={292} title="Summary">
        <RequestMetadata data={request} />
      </Panel>
      <Panel title="Requests" headerControls={
            <ButtonGroup>
              <Button hidden={!!statusUpdating || Object.keys(requestedApproveRejectState[0]).length === 0} kind="secondary" onClick={handleReviewRequestCancel}>Cancel</Button>
              <Button hidden={!!statusUpdating || Object.keys(requestedApproveRejectState[0]).length === 0} kind="primary" onClick={handleReviewRequestResolve}>Save Changes</Button>
            </ButtonGroup>
        }>
        <HeightPanelContent>
          <ReviewRequestTable requestsType="requests" />
        </HeightPanelContent>
      </Panel>
    </PanelGroup>
  );
}
const RequestMetadata: React.FC<{ data?: IConflictingCommandsResponseDto }> = ({
  data,
}) => {
  if (!data) return;

  return (
    <div style={{ display: "flex", flexDirection: "column" }}>
      <StyledDivKey>Capability</StyledDivKey>
      <StyledDivValue>{data.twinId}</StyledDivValue>
      <StyledDivKey>Asset</StyledDivKey>
      <StyledDivValue>
        <TwinChip
          value={data.isCapabilityOf ?? data.isHostedBy ?? "Unknown"}
          type="asset"
        />
      </StyledDivValue>
      <StyledDivKey>Location</StyledDivKey>
      <StyledDivValue>
        <TwinChip value={data.location ?? "Unknown"} type="site" />
      </StyledDivValue>
      <StyledDivKey>Present Value</StyledDivKey>
      <StyledDivValue>{!!data.presentValue ? `${data.presentValue}${data.unit}` : "-"}</StyledDivValue>
    </div>
  );
};

const divStyle = {
  minWidth: 150,
  padding: "8px",
  fontSize: "12px",
};

const StyledDivKey = styled("div")({
  width: 150,
  ...divStyle,
  color: "#919191",
});

const StyledDivValue = styled("div")({ ...divStyle, paddingTop: 0, color: "#C6C6C6" });
