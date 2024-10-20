import { Modal, useTheme } from "@willowinc/ui";
import styled from "@emotion/styled";
import TwinChip from "../../../components/TwinChip/TwinChip";
import { ActivityLogsResponseDto, ActivityType } from "../../../services/Clients";
import React from "react";
import { useActivityLogs } from "../ActivityLogsProvider";
import { formatDate } from "../../../utils/formatDate";
import { ActivityLogMessage } from "./ActivityLogMessage";
import { ActivityLogRole } from "./ActivityLogRole";

export const ActivityLogModal: React.FC = () => {

  const { modalState, selectedRowState } = useActivityLogs();
  const [opened, { close }] = modalState;

  function getTitle(log?: ActivityLogsResponseDto) {
    switch (log?.type) {
      case ActivityType.Received:
        return "Command Received";
      case ActivityType.Approved:
      case ActivityType.Cancelled:
      case ActivityType.Executed:
      case ActivityType.Failed:
      case ActivityType.Retracted:
      case ActivityType.Completed:
      case ActivityType.Retried:
        return log.type.toString();
      case ActivityType.MessageSent:
        return "Write Request";
      case ActivityType.MessageReceivedFailed:
        return "Failed";
      case ActivityType.MessageReceivedSuccess:
        return "Success";
      default:
        return null;
    }

  }

  return (
    <Modal
      size="xl"
      closeOnClickOutside={false}
      centered
      opened={opened}
      onClose={close}
      header={`Activity Log - ${getTitle(selectedRowState[0])}`}
    >
      <div className="p-4">
        <TwoColumnTable data={selectedRowState[0]} />
      </div>
    </Modal>
  );
}

const TwoColumnTable: React.FC<{ data?: ActivityLogsResponseDto }> = ({
  data,
}) => {
  if (!data) return;

  const theme = useTheme();
  return (
    <>
      {/*@ts-expect-error*/}
      <Container showBorder={data.extraInfo}>
        <StyledTable>
          <tbody>
            <tr>
              <StyledTdKey>Date</StyledTdKey>
              <StyledTdValue>{formatDate(data.timestamp)}</StyledTdValue>
            </tr>
            <tr>
              <StyledTdKey>Activity</StyledTdKey>
              <StyledTdValue>
                <ActivityLogMessage activityLog={data} />
              </StyledTdValue>
            </tr>
            <tr>
              <StyledTdKey>Role</StyledTdKey>
              <StyledTdValue>
                <ActivityLogRole activityLog={data} />
              </StyledTdValue>
            </tr>
            <tr>
              <StyledTdKey>Command</StyledTdKey>
              <StyledTdValue>{!!data.resolvedCommandId ? <a href={`/commands/${data.resolvedCommandId}`}>{data.commandName ?? "Unknown"}</a> : data.commandName ?? "Unknown"}</StyledTdValue>
            </tr>
            <tr>
              <StyledTdKey>Asset</StyledTdKey>
              <StyledTdValue>
                <TwinChip type="asset" value={data.isCapabilityOf ?? data.isHostedBy ?? "Unknown"} />
              </StyledTdValue>
            </tr>
            <tr>
              <StyledTdKey>Location</StyledTdKey>
              <StyledTdValue>
                <TwinChip type="site" value={data.location ?? "Unknown"} />
              </StyledTdValue>
            </tr>
          </tbody>
        </StyledTable>
      </Container>
      {data.extraInfo &&
        <SecondStyledTable>
          <tbody>
            <tr>
              <StyledTdKey>Request Data</StyledTdKey>
              <StyledTdValue>
                {/*@ts-expect-error*/}
                <StyledPre bg={theme.color.neutral.bg.accent.default}>{data.extraInfo}</StyledPre>
              </StyledTdValue>
            </tr>
          </tbody>
        </SecondStyledTable>
      }
    </>
  );
};

const tdStyle = {
  padding: "2px 0",
  fontSize: "12px",
  lineHeight: "30px",
  verticalAlign: "top",
};

const StyledTdKey = styled("td")({
  ...tdStyle,
  minWidth: 150,
  width: 200,
  color: "#919191",
});

const StyledTdValue = styled("td")({
  ...tdStyle,
  width: "100%",
  color: "#C6C6C6",
  display: "flex",
  alignItems: "center",
});

const StyledTable = styled("table")({
  width: "100%",
});

const SecondStyledTable = styled("table")({
  width: "100%",
  marginTop: "1em",
});

const Container = styled("div")(props => ({
  width: "100%",
  //@ts-expect-error
  paddingBottom: props.showBorder ? "1em" : "0",
  //@ts-expect-error
  borderBottom: props.showBorder ? "1px solid #3B3B3B" : "none",
}));


const StyledPre = styled("pre")(props => ({
  lineHeight: "1.5em",
  //@ts-expect-error
  backgroundColor: props.bg,
  width: "100%",
  padding: "1em",
}));
