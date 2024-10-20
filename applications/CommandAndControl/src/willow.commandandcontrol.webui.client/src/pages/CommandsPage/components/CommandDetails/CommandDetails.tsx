import { PanelGroup, Panel, PanelContent, Icon, IconName } from "@willowinc/ui";
import styled from "@emotion/styled";
import { useCommands } from "../../CommandsProvider";
import TwinChip, { TwinChipType } from "../../../../components/TwinChip/TwinChip";
import { ActivityLogsTable } from "../../../ActivityLogsPage/components/ActivityLogsTable";
import { ActivityLogsProvider } from "../../../ActivityLogsPage/ActivityLogsProvider";
import { IResolvedCommandResponseDto } from "../../../../services/Clients";
import React from "react";
import { formatDate } from "../../../../utils/formatDate";
import { ActivityLogModal } from "../../../ActivityLogsPage/components/ActivityLogModal";
import { HeightPanelContent } from "../../../../components/Styled/HeightPanelContent";

export default function CommandDetails() {

  const { selectedRowState } = useCommands();

  const command = selectedRowState[0];

  if (!command) return null;

  return (
    <PanelGroup units="percentages" resizable>
      <Panel collapsible title="Summary">
        <Details command={command} />
        <Location command={command} />
      </Panel>
      <Panel collapsible title="Activity">
        <HeightPanelContent>
          <ActivityLogsProvider compact commandId={command.id} requestId={command.requestId}>
            <ActivityLogsTable />
            <ActivityLogModal />
          </ActivityLogsProvider>
        </HeightPanelContent>
      </Panel>
    </PanelGroup>
  );
}
const Details: React.FC<{ command?: IResolvedCommandResponseDto | undefined }> = ({ command }) => {
  if (!command) return;
  
  return (
    <BorderedContainer>
      <HeaderContainer>
        <HeaderLabel>
          <Icon icon="subject" size={20} />
          <LabelText>Details</LabelText>
        </HeaderLabel>
      </HeaderContainer>
      <StyledTable>
        <tbody>
          <tr>
            <StyledTdKey>Capability</StyledTdKey>
            <StyledTdValue>{command.twinId}</StyledTdValue>
          </tr>
          <tr>
            <StyledTdKey>Asset</StyledTdKey>
            <StyledTdValue>
              <TwinChip value={command.isCapabilityOf ?? command.isHostedBy ?? "Unknown"} type="asset" />
            </StyledTdValue>
          </tr>
          <tr>
            <StyledTdKey>Command</StyledTdKey>
            <StyledTdValue>{command.commandName}</StyledTdValue>
          </tr>
          <tr>
            <StyledTdKey>Present Value</StyledTdKey>
            <StyledTdValue>{command.presentValue ? `${command.presentValue}${command.unit}` : "-"}</StyledTdValue>
          </tr>
          <tr>
            <StyledTdKey>Target Value</StyledTdKey>
            <StyledTdValue>{command.value}{command.unit}</StyledTdValue>
          </tr>
          <tr>
            <StyledTdKey>Start Time</StyledTdKey>
            <StyledTdValue>{formatDate(command.startTime)}</StyledTdValue>
          </tr>
          <tr>
            <StyledTdKey>End Time</StyledTdKey>
            <StyledTdValue>{!!command.endTime ? formatDate(command.endTime) : "-"}</StyledTdValue>
          </tr>
        </tbody>
      </StyledTable>
    </BorderedContainer>
  );
};

const Location: React.FC<{ command: IResolvedCommandResponseDto | undefined}> = ({command}) => (
  <Container>
    <HeaderContainer>
      <HeaderLabel>
        <Icon icon="location_on" size={20} />
        <LabelText>Location</LabelText>
      </HeaderLabel>
    </HeaderContainer>
    <StyledTable>
      <tbody>
        {command?.locations?.sort((a, b) => a.order! > b.order! ? 1 : -1).map((location, index) => (
        <tr>
          <StyledTdKey>{location.model}</StyledTdKey>
          <StyledTdValue>
            <TwinChip value={location.locationTwinId!} type={location.model?.toLocaleLowerCase() as TwinChipType} />
          </StyledTdValue>
        </tr>
))}
      </tbody>
    </StyledTable>
  </Container>
);

const Container = styled("div")({
  display: "flex",
  flexDirection: "column",
  width: "100%",
  height: "50%",
});

const BorderedContainer = styled(Container)({
  borderBottom: "1px solid #3B3B3B",
});

const Section = ({
  icon,
  label,
  data,
  className,
}: {
  icon: IconName;
  label: string;
  data: Record<string, any>;
  className?: string;
}) => {
  return (
    <HeaderContainer className={className}>
      <HeaderLabel>
        <Icon icon={icon} size={20} />
        <LabelText>{label}</LabelText>
      </HeaderLabel>
      <TwoColumnTable data={data} type={label} />
    </HeaderContainer>
  );
};

const HeaderContainer = styled("div")({
  display: "flex",
  flexDirection: "column",
  gap: 12,
  padding: 16,
});

const HeaderLabel = styled("div")({
  display: "flex",
  flexDirection: "row",
  color: "#C6C6C6",
  gap: 10,
});

const LabelText = styled("div")({
  font: " 500 14px/20px Poppins",
  letterSpacing: 0,
  textAlign: "left",
});

const TwoColumnTable = ({ data, type }: any) => {
  const keyFieldMap = {
    twin: "Twin",
    command: "Command",
    originalValue: "Original Value",
    presentValue: "Present Value",
    targetValue: "Target Value",
    startTime: "Start Time",
    endTime: "End Time",

    room: "Room",
    level: "Level",
    zone: "Zone",
    site: "Site",
  } as any;

  return (
    <StyledTable>
      <tbody>
        {Object.entries(data).map(([key, value]) => (
          <tr key={key}>
            <StyledTdKey>{keyFieldMap[key] as any}</StyledTdKey>
            <StyledTdValue>
            </StyledTdValue>
          </tr>
        ))}
      </tbody>
    </StyledTable>
  );
};

const tdStyle = {
  minWidth: 150,
  width: 0,
  padding: "8px",
  fontSize: "12px",
};

const StyledTdKey = styled("td")({
  ...tdStyle,
  color: "#919191",
});

const StyledTdValue = styled("td")({ ...tdStyle, color: "#C6C6C6" });
const StyledTable = styled("table")({ width: "50%" });
