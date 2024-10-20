
import { DatesRangeValue } from "@mantine/dates";
import { Icon, DateTimeInput } from "@willowinc/ui";
import { Dispatch, SetStateAction } from "react";
import ReactDOM from "react-dom";
import styled from "styled-components";

export function DateRangePickerPortal({
  dateRangeState,
}: {
  dateRangeState: [
    DatesRangeValue,
    Dispatch<SetStateAction<[Date | null, Date | null]>>
  ];
}) {

  return ReactDOM.createPortal(
    <StyledDateTimeInput
      prefix={<Icon icon="calendar_today" />}
      type="date-time-range"
      value={dateRangeState[0] as [Date, Date]}
      onChange={(val) => {
        //@ts-ignore
        if (!val[0] && !val[1]) return;
        dateRangeState[1](val as DatesRangeValue);
      }}
    />,
    document.getElementById("date-range-picker-portal")!
  );
}

const StyledDateTimeInput = styled(DateTimeInput)({
  width: "350px",
})
