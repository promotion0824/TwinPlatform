import styled from "@emotion/styled";
import { Icon, IconButton, Loader } from "@willowinc/ui";
import { useState } from "react";

import { ActivityStatus } from "../../../../components/Activity/ActivityStatus";
import TwinChip from "../../../../components/TwinChip/TwinChip";
import ErrorMessage from "../../../../components/error/ErrorMessage";
import {  IActivityLogsResponseDto } from "../../../../services/Clients";
import { formatDate } from "../../../../utils/formatDate";
import { useOverviewContext } from "../../OverviewProvider";
import { CenterContainer } from "../../../../components/Styled/CenterContainer";
import { ActivityLogRole } from "../../../ActivityLogsPage/components/ActivityLogRole";
import { ActivityLogMessage } from "../../../ActivityLogsPage/components/ActivityLogMessage";

export default function Activity() {
  const { getStatisticsQuery } = useOverviewContext();

  const { data, isLoading, isError, isSuccess } = getStatisticsQuery;

  const { recentActivities = [] } = data || {};

  if (isLoading)
    return (
      <CenterContainer>
        <Loader size="md" />
      </CenterContainer>
    );

  if (isError)
    return (
      <CenterContainer>
        <ErrorMessage />
      </CenterContainer>
    );

  if (recentActivities.length === 0 && isSuccess)
    return (
      <CenterContainer>
        <NoActivitiesText>No Activity</NoActivitiesText>
      </CenterContainer>
    );

  return (
    <ActivityContainer>
      {recentActivities.map((item, i) => {
        return (
          <div key={`${item.commandName}${item.connectorId}${i}`}>
            <ActivityItem item={item} expanded={i == 0} />
          </div>
        );
      })}
    </ActivityContainer>
  );
}

const NoActivitiesText = styled("div")({
  font: "500 1rem/1.25rem Poppins",
  color: "#919191",
});


const ActivityContainer = styled("div")({
  padding: 16,
  height: "100%",
  width: "100%",
  display: "flex",
  flexDirection: "column",
  gap: 16,
  overflowY: "auto",
});

function ActivityItem({ item, expanded = false }: { item: IActivityLogsResponseDto, expanded?: boolean }) {
  const [isExpanded, setIsExpanded] = useState(expanded);

  const { timestamp, type, commandName } = item || {};

  return (
    <div>
      {/* @ts-ignore */}
      <Container isExpanded={isExpanded}>
        <LeftContainer>
          <Text>{commandName}</Text>
          <ActivityStatus activityType={type!} />
        </LeftContainer>

        <RightContainer>
          <TimeText>{formatDate(timestamp)}</TimeText>
          <BorderlessButton
            background="transparent"
            kind="secondary"
            onClick={() => {
              setIsExpanded((prev) => !prev);
            }}
          >
            <Icon icon={(isExpanded ? "expand_less" : "expand_more") as any} />
          </BorderlessButton>
        </RightContainer>
      </Container>

      {isExpanded && (
        <ExpandedContainer>
          <TwoColumnTable item={item} />
        </ExpandedContainer>
      )}
    </div>
  );
}

const FlexDiv = styled("div")({
  display: "flex",

});

const Container = styled("div")(({ isExpanded }: { isExpanded: boolean }) => ({
  width: "100%",
  height: 52,
  display: "flex",
  flexDirection: "row",
  justifyContent: "space-between",

  padding: "12px 16px",
  borderRadius: isExpanded ? "4px 4px 0px 0px" : "4px",
  border: "1px solid #3B3B3B",
  gap: "4px",
  backgroundColor: "#2C2C2C",
}));

const LeftContainer = styled("div")({
  display: "flex",
  flexDirection: "row",
  gap: 4,
  alignItems: "center",
  flex: 1,
});

const RightContainer = styled("div")({
  display: "flex",
  flexDirection: "row",
  gap: 8,
  alignItems: "center",
});

const noOverflowText = {
  textOverflow: "ellipsis",
  textAlign: "left",
  maxHeight: 52,
  overflow: "hidden",
  whiteSpace: "nowrap",
};

// @ts-ignore
const Text = styled("div")({
  ...noOverflowText,
  color: "#C6C6C6",
  font: " normal 500 13px/20px Poppins",
  maxWidth: "250px",
});

// @ts-ignore
const TimeText = styled("div")({
  ...noOverflowText,
  color: "#919191",
  font: "normal 400 12px/20px Poppins",
  textAlign: "left",
  flex: 2,
});

const BorderlessButton = styled(IconButton)({
  outline: "none !important",
  "&:hover": { backgroundColor: "transparent !important" },
});

const ExpandedContainer = styled("div")({

   width: "100%",
  border: "1px solid #3B3B3B",

  backgroundColor: "#242424",
  borderTop: "none",

  padding: 16,
  overflowY: "auto",
  whiteSpace: "nowrap",
});

const TwoColumnTable = ({ item }: any) => {
  const keyFieldMap = new Map([
    ["Activity", "activity"],
    ["Role", "role"],
    ["Asset", "isCapabilityOf"],
    ["Command", "commandName"],
    ["Target Value", "value"],
    ["Start Time", "startTime"],
    ["End Time", "endTime"],
  ]);

  function renderValue(key: any) {
    let value = item[key];

    switch (true) {
      case Object.prototype.toString.call(value) === "[object Date]":
        return formatDate(value);

      case key === "value":
        return `${value}${item.unit}`;

      case key === "isCapabilityOf":
        return <TwinChip type="asset" value={value} />;

      case key === "activity":
        return <ActivityLogMessage activityLog={item} />;

      case key === "role":
        return <FlexDiv><ActivityLogRole activityLog={item} /></FlexDiv>;

      default:
        return value;
    }
  }

  return (
    <StyledTable>
      <tbody>
        {[...keyFieldMap.entries()].map(([fieldName, key]) => {
          return (
            <tr key={key}>
              <StyledTdKey>{fieldName}</StyledTdKey>
              <StyledTdValue>{renderValue(key)}</StyledTdValue>
            </tr>
          );
        })}
      </tbody>
    </StyledTable>
  );
};

const tdStyle = {
  paddingBottom: 10,
  fontSize: "12px",
};

const StyledTdKey = styled("td")({
  ...tdStyle,
  color: "#919191",
});

const StyledTdValue = styled("td")({ ...tdStyle });
const StyledTable = styled("table")({ width: "100%" });
