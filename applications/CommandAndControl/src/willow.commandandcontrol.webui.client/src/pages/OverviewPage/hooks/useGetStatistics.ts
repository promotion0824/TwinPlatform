import { useState, Dispatch, SetStateAction } from "react";
import { useQuery, UseQueryResult } from "@tanstack/react-query";
import { ApiException, GetStatisticsResponseDto, GetStatisticsRequestDto, } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";
import { DatesRangeValue } from "@mantine/dates";
import { useAppContext } from "../../../providers/AppContextProvider";
import { UseQueryOptions } from "../../../../types/UseQueryOptions";

export interface IGetStatistic {
  query: UseQueryResult<GetStatisticsResponseDto, ApiException>;
  siteIdState: [
    string | undefined,
    Dispatch<SetStateAction<string | undefined>>
  ];
  dateRangeState: [
    DatesRangeValue,
    Dispatch<SetStateAction<[Date | null, Date | null]>>
  ];
}
// Get the current date
const currentDate = new Date();

// Get the date 7 days ago
const sevenDaysAgo = new Date();
sevenDaysAgo.setDate(currentDate.getDate() - 7);

export default function useGetStatistics(
  options?: UseQueryOptions<GetStatisticsResponseDto>
) {
  const api = useApi();
  const { selectedSite } = useAppContext();

  const dateRangeState = useState<[Date | null, Date | null]>([
    sevenDaysAgo,
    currentDate,
  ]);

  const body = new GetStatisticsRequestDto();

  body.siteId =
    selectedSite === "allSites" ? undefined : selectedSite;
  body.startDate = dateRangeState[0][0]!;
  body.endDate = dateRangeState[0][1]!;

  const query = useQuery<GetStatisticsResponseDto, ApiException>({
    queryKey: ["Statistics", selectedSite, dateRangeState[0]],
    queryFn: () => api.getStatistics(body),
    ...options,
    enabled:
      !!selectedSite &&
      !!dateRangeState[0][0] &&
      !!dateRangeState[0][1],
  });

  return {
    query,
    dateRangeState,
  };
}
