import styled from "@emotion/styled";
import { Select, CheckboxGroup, Checkbox, SearchInput } from "@willowinc/ui";
import { useEffect, useState } from "react";
import { FilterSpecificationDto, ResolvedCommandStatus } from "../../../../services/Clients";
import addDays from "date-fns/addDays";
import { useDebounce } from "use-debounce";
import { getSearchInputFilterSpecification } from "../../../../utils/getSearchInputFilterSpecification";
import { useAppContext } from "../../../../providers/AppContextProvider";

export default function CommandsFilters() {
  return (
    <FiltersContainer>
      <SearchFilter />
      <DateFilter />
      <CommandStatusFilter />
    </FiltersContainer>
  );
}

function CommandStatusFilter({ }) {
  const { commandsFilters } = useAppContext();
  const [statusState, setStatusState] = useState<string[]>(commandsFilters[0].status?.flatMap((x) => x.value) ?? []);

  const statusLabelMap = [
    ResolvedCommandStatus.Approved,
    ResolvedCommandStatus.Scheduled,
    ResolvedCommandStatus.Cancelled,
    ResolvedCommandStatus.Failed,
    ResolvedCommandStatus.Executing,
    ResolvedCommandStatus.Executed,
  ];

  useEffect(() => {

    if (statusState.length === 0) {
      commandsFilters[1]((prev) => ({ ...prev, status: undefined }));
      return;
    }

    const result = new FilterSpecificationDto();
    result.field = "status";
    result.logicalOperator = "AND";
    result.operator = "in";
    result.value = statusState;

    commandsFilters[1]((prev) => ({ ...prev, status: [result] }));
  }, [statusState]);

  return (
    <CheckboxGroup
      label="Command status"
      value={statusState}
      onChange={(values) => {
        setStatusState(values);
      }}
    >
      {statusLabelMap.map((label) => (
        <Checkbox key={label} label={label} value={label} />
      ))}
    </CheckboxGroup>
  );
}
const FiltersContainer = styled("div")({
  display: "flex",
  flexDirection: "column",
  gap: 16,
  padding: 16,
});

function SearchFilter() {
  const { commandsFilters } = useAppContext();

  // fields to filter by
  const fields = ["commandName", "twinId"];

  const [search, setSearch] = useState(commandsFilters[0].search && commandsFilters[0].search[0] ? commandsFilters[0].search[0].value : "");

  const [debouncedSearch] = useDebounce(search, 500);

  useEffect(() => {
    if (debouncedSearch) {
      commandsFilters[1]((prev) => ({
        ...prev,
        search: getSearchInputFilterSpecification(debouncedSearch, fields),
      }));
    } else {
      commandsFilters[1]((prev) => ({
        ...prev,
        search: undefined,
      }));
    }

  }, [debouncedSearch]);

  return (
    <>
      <SearchInput
        label="Search"
        value={search}
        placeholder="Filter all commands"
        onChange={(e) => setSearch(e.target.value)} />
    </>
  );
}

function DateFilter() {

  const { commandsFilters, commandsSelectedDate } = useAppContext();

  return (
    <Select label="Approved Date" placeholder="Select Date" clearable onChange={(e) => {

      if (e == null) {
        commandsFilters[1]((prev) => ({
          ...prev,
          startDate: undefined,
          endDate: undefined,
        }));
        commandsSelectedDate[1](undefined);
        return;
      }

      commandsSelectedDate[1](e);
      var days = Number(e);

      var currentDate = new Date();

      var rangeEndDate = addDays(currentDate, days);

      const createdDate = new FilterSpecificationDto();
      createdDate.field = "createdDate";
      createdDate.logicalOperator = "AND";
      createdDate.operator = ">=";
      createdDate.value = days > 0 ? currentDate : rangeEndDate;

      commandsFilters[1]((prev) => ({
        ...prev,
        startDate: [createdDate],
      }));
    }}
      value={commandsSelectedDate[0]}
      data={[
        {
          label: "Next 7 Days",
          value: "7",
        },
        {
          label: "Last 24 Hours",
          value: "-1",
        },
        {
          label: "Last 7 Days",
          value: "-7",
        },
        {
          label: "Last 30 Days",
          value: "-30",
        },
        {
          label: "Last Year",
          value: "-365",
        },
        {
          label: "Last Two Years",
          value: "-730",
        },
      ]} />
  )
}
