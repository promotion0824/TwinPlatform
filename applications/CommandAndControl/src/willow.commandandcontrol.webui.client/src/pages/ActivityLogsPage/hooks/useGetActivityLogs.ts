import { useState } from "react";
import { InfiniteData, QueryKey,  useInfiniteQuery, UseInfiniteQueryResult, UseQueryResult } from "@tanstack/react-query";
import { ApiException, SortSpecificationDto, FilterSpecificationDto, ActivityLogsRequestDto, ActivityLogsResponseDtoBatchDto, ActivityLogsResponseDto } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";
import { useAppContext } from "../../../providers/AppContextProvider";
import { UseState } from "../../../../types/UseState";
import { UseInfiniteQueryOptions } from "../../../../types/UseInfiniteQueryOptions";
import { useSiteFilter } from "../../../hooks/useSiteFilter";
import { toFlatArray } from "../../../utils/toArray";

export interface GetActivityLogs {
  query: UseInfiniteQueryResult<InfiniteData<ActivityLogsResponseDtoBatchDto, number>, ApiException>;
  sortState: UseState<SortSpecificationDto[]>;
}

export const useGetActivityLogs = (
  defaultFilters: FilterSpecificationDto[] = [],
  options?: UseInfiniteQueryOptions<ActivityLogsResponseDtoBatchDto>,
) => {
  const api = useApi();
  const { selectedSite, activityLogsFilters } = useAppContext();
  const siteFilter = useSiteFilter();

  const sortState = useState<SortSpecificationDto[]>([new SortSpecificationDto({ field: "timestamp", sort: "desc" })]);

  const body = new ActivityLogsRequestDto();

  const filterSpecifications = [
    ...toFlatArray(activityLogsFilters[0]),
    ...defaultFilters,
    siteFilter,
  ].filter((x) => x) as FilterSpecificationDto[];

  body.filterSpecifications = filterSpecifications;
  body.pageSize = 100;
  body.sortSpecifications = sortState[0];

  const query = useInfiniteQuery<ActivityLogsResponseDtoBatchDto, ApiException, InfiniteData<ActivityLogsResponseDtoBatchDto, number>, QueryKey, number>({
    queryKey: [
      "activityLogs",
      sortState[0],
      JSON.stringify(filterSpecifications),
      selectedSite,
    ],
    queryFn: ({ pageParam }) => {
      body.page = pageParam;
      return api.activityLogs(body);
    },
    ...options,
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages, lastPageParam) => {
      if (allPages.flatMap(a => a.items ?? []).length < lastPage.total!) {
        return lastPageParam + 1;
      }
    },
  });

  return {
    query,
    sortState,
  };
}
