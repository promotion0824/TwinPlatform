import styled from "@emotion/styled";
import { Checkbox, CheckboxGroup, SearchInput, Select, TextInput } from "@willowinc/ui";
import { ActivityType, FilterSpecificationDto } from "../../../services/Clients";
import { useDebounce } from "use-debounce";
import { useEffect, useState } from "react";
import addDays from "date-fns/addDays";
import { getSearchInputFilterSpecification } from "../../../utils/getSearchInputFilterSpecification";
import { useAppContext } from "../../../providers/AppContextProvider";
import { getFilterSpecification } from "../../../utils/getFilterSpecification";
import { AppName } from "../../../utils/appName";

export const ActivityLogsFilters = () => {
  return (
    <FiltersContainer>
      <SearchFilter />
      <DateFilter />
      <ActivityTypeFilter />
    </FiltersContainer>
  );
}

const FiltersContainer = styled("div")({
  display: "flex",
  flexDirection: "column",
  gap: 16,
  padding: 16,
});

function SearchFilter() {
  const { activityLogsFilters } = useAppContext();

  // fields to filter by
  const fields = ["commandName", "isCapabilityOf", "isHostedBy"];

  const [search, setSearch] = useState<string>(activityLogsFilters[0].search && activityLogsFilters[0].search[0] ? activityLogsFilters[0].search[0].value : "");

  const [debouncedSearch] = useDebounce(search, 500);

  useEffect(() => {
    if (debouncedSearch) {
      activityLogsFilters[1]((prev) => ({
        ...prev,
        search: getSearchInputFilterSpecificationForActivityLog(debouncedSearch, fields),
      }));
    } else {
      activityLogsFilters[1]((prev) => ({
        ...prev,
        search: undefined,
      }));
    }

  }, [debouncedSearch]);

  return (
    <SearchInput
      label="Search"
      value={search}
      placeholder="Filter all requests"
      onChange={(e) => setSearch(e.target.value)} />
  );
}

function DateFilter() {

  const { activityLogsFilters, activityLogsSelectedDate } = useAppContext();

  return (
    <Select label="Request Date" placeholder="Select Date" clearable onChange={(e) => {

      if (e == null) {
        activityLogsFilters[1]((prev) => ({
          ...prev,
          startDate: undefined,
          endDate: undefined,
        }));
        activityLogsSelectedDate[1](undefined);
        return;
      }

      activityLogsSelectedDate[1](e);
      const days = Number(e);

      const currentDate = new Date(new Date().toDateString());

      const rangeEndDate = addDays(currentDate, -days);

      const startTime = new FilterSpecificationDto();
      startTime.field = "timestamp";
      startTime.logicalOperator = "AND";
      startTime.operator = ">=";
      startTime.value = rangeEndDate;

      activityLogsFilters[1]((prev) => ({
        ...prev,
        startDate: [startTime],
      }));
    }}
    value={activityLogsSelectedDate[0]}
      data={[
        {
          label: "Last 24 Hours",
          value: "1",
        },
        {
          label: "Last 7 Days",
          value: "7",
        },
        {
          label: "Last 30 Days",
          value: "30",
        },
        {
          label: "Last Year",
          value: "365",
        },
        {
          label: "Last Two Years",
          value: "730",
        },
      ]} />
  )
}

function ActivityTypeFilter({ }) {
  const { activityLogsFilters, activityLogsSelectedTypes } = useAppContext();
  const [statusState, setStatusState] = useState<string[]>(activityLogsSelectedTypes[0] ?? []);

  const statusLabelMap: any[] = [
    "Activate",
    AppName,
    "Edge Controller",
    "User Action",
  ];

  useEffect(() => {

    const statuses: ActivityType[] = [];

    activityLogsSelectedTypes[1](statusState);

    statusState.forEach((status) => {
      switch (status) {
        case "Activate":
          statuses.push(ActivityType.Received);
          break;
        case AppName:
          statuses.push(ActivityType.MessageSent);
          statuses.push(ActivityType.Failed);
          break;
        case "Edge Controller":
          statuses.push(ActivityType.MessageReceivedSuccess);
          statuses.push(ActivityType.MessageReceivedFailed);
          break;
        case "User Action":
          statuses.push(ActivityType.Approved);
          statuses.push(ActivityType.Cancelled);
          statuses.push(ActivityType.Executed);
          statuses.push(ActivityType.Retracted);
          statuses.push(ActivityType.Suspended);
          statuses.push(ActivityType.Retried);
          break;
      }
    });

    if (statuses.length === 0) {
      activityLogsFilters[1]((prev) => ({ ...prev, status: undefined }));
      return;
    }

    const result = new FilterSpecificationDto();
    result.field = "type";
    result.logicalOperator = "AND";
    result.operator = "in";
    result.value = statuses;

    activityLogsFilters[1]((prev) => ({ ...prev, status: [result] }));
  }, [statusState]);
  return (
    <CheckboxGroup
      label="Activity Type"
      onChange={(values) => {
        setStatusState(values);
      }}
      value={statusState}
    >
      {statusLabelMap.map((label) => (
        <Checkbox key={label} label={label} value={label} />
      ))}
    </CheckboxGroup>
  );
}

const activityTypes = Object.keys(ActivityType);

const getSearchInputFilterSpecificationForActivityLog = (search: string, fields: string[]) => {

  const filterSpecs: FilterSpecificationDto[] = getSearchInputFilterSpecification(search, fields);

  const matchingActivityTypes = activityTypes.filter((type) => type.toLowerCase().includes(search.toLowerCase()));

  for (const activityType of matchingActivityTypes) {
    filterSpecs.push(getFilterSpecification("type", "OR", "=", activityType));
  }

  return filterSpecs;
};
