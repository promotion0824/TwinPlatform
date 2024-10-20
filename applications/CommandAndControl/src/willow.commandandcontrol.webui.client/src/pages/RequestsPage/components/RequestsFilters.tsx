import styled from "@emotion/styled";
import { SearchInput, Select, TextInput } from "@willowinc/ui";
import { FilterSpecificationDto } from "../../../services/Clients";
import { useDebounce } from "use-debounce";
import { useEffect, useState } from "react";
import addDays from "date-fns/addDays";
import { getSearchInputFilterSpecification } from "../../../utils/getSearchInputFilterSpecification";
import { useAppContext } from "../../../providers/AppContextProvider";

export default function RequestsFilters() {
  return (
    <FiltersContainer>
      <SearchFilter />
      <DateFilter />
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
   const { requestsFilters } = useAppContext();

  // fields to filter by
  const fields = ["twinId"];

  const [search, setSearch] = useState(requestsFilters[0].search && requestsFilters[0].search[0] ? requestsFilters[0].search[0].value : "");

  const [debouncedSearch] = useDebounce(search, 500);

  useEffect(() => {
    if (debouncedSearch) {
      requestsFilters[1]((prev) => ({
        ...prev,
        search: getSearchInputFilterSpecification(debouncedSearch, fields),
      }));
    } else {
      requestsFilters[1]((prev) => ({
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
        placeholder="Filter all requests"
        onChange={(e) => setSearch(e.target.value)} />
    </>
  );
}

function DateFilter() {

  const { requestsFilters, requestsSelectedDate } = useAppContext();

  return (
    <Select label="Received Date" placeholder="Select Date" clearable onChange={(e) => {

      if (e == null) {
        requestsFilters[1]((prev) => ({
          ...prev,
          startDate: undefined,
          endDate: undefined,
        }));
        requestsSelectedDate[1](undefined);
        return;
      }

      requestsSelectedDate[1](e);
      const days = Number(e);

      const currentDate = new Date(new Date().toDateString());

      const rangeEndDate = addDays(currentDate, -days);
      
      const startTime = new FilterSpecificationDto();
      startTime.field = "receivedDate";
      startTime.logicalOperator = "AND";
      startTime.operator = ">=";
      startTime.value = rangeEndDate;

      requestsFilters[1]((prev) => ({
        ...prev,
        startDate: [startTime],
      }));
    }}
    value={requestsSelectedDate[0]}
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
