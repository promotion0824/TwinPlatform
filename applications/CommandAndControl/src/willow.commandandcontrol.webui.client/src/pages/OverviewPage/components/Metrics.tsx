import { Loader } from "@willowinc/ui";
import styled from "styled-components";
import { useOverviewContext } from "../OverviewProvider";

export default function Metrics() {
  const { getStatisticsQuery } = useOverviewContext();
  const { data, isLoading, isError } = getStatisticsQuery;
  const { commandsCount } = data || {};

  const {
    totalApprovedCommands = 0,
    totalCancelledCommands = 0,
    totalExecutedCommands = 0,
    totalFailedCommands = 0,
    totalRequestedCommands = 0,
    totalSuspendedCommands = 0,
  } = commandsCount || {};

  const metricsObject = {
    Received: totalRequestedCommands,
    Approved: totalApprovedCommands,
    Cancelled: totalCancelledCommands,
    Executed: totalExecutedCommands,
    Failed: totalFailedCommands,
    Suspended: totalSuspendedCommands,
  };

  return (
    <Container>
      {Object.entries(metricsObject).map(([name, value]) => (
        <Metric
          // @ts-ignore
          key={name}
          name={name}
          value={isError ? "?" : value}
          isLoading={isLoading || false}
        />
      ))}
    </Container>
  );
}

const Container = styled("div")({
  display: "flex",
  flexDirection: "row",
  gap: 12,
  paddingBottom: 12,
  justifyContent: "space-between",
});

function Metric({
  name,
  value,
  isLoading,
}: {
  name: string;
  value: number | string;
  isLoading: boolean;
}) {
  return (
    <MetricCard>
      <TextContainer>
        <OverFlowText>{name}</OverFlowText>
        <MetricValueText>
          {isLoading ? <Loader size="lg" variant="dots" /> : value}
        </MetricValueText>
      </TextContainer>
    </MetricCard>
  );
}

const MetricCard = styled("div")({
  display: "flex",
  flexDirection: "column",
  borderRadius: "2px",
  border: "1px solid  #3B3B3B",
  background: "#242424",
  width: "100%",
  minHeight: 94,
});

const TextContainer = styled("div")({
  display: "flex",
  flexDirection: "column",
  gap: 8,
  alignItems: "flex-start",
  padding: "12px 16px",
});

const OverFlowText = styled("span")({
  textOverflow: "ellipsis",
  overflow: "hidden",
  whiteSpace: "nowrap",
  color: "#C6C6C6",
  font: "normal 500 13px/20px Poppins",
});

const MetricValueText = styled("div")({
  font: "normal 500 32px/40px Poppins",
});
