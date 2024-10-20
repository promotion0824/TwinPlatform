import { useState } from "react";
import { InfiniteData, QueryKey, useInfiniteQuery, UseInfiniteQueryResult } from "@tanstack/react-query";
import { ApiException, GetConflictingCommandsRequestDto, SortSpecificationDto, FilterSpecificationDto, ConflictingCommandsResponseDtoBatchDto } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";
import { useAppContext } from "../../../providers/AppContextProvider";
import { UseState } from "../../../../types/UseState";
import { UseInfiniteQueryOptions } from "../../../../types/UseInfiniteQueryOptions";
import { toFlatArray } from "../../../utils/toArray";
import { useSiteFilter } from "../../../hooks/useSiteFilter";

export interface IGetRequestedCommands {
  query: UseInfiniteQueryResult<InfiniteData<ConflictingCommandsResponseDtoBatchDto, number>, ApiException>;
  sortState: UseState<SortSpecificationDto[]>;
}

export default function useGetRequestedCommands(
  type: "newRequests" | "closedRequests",
  options?: UseInfiniteQueryOptions<ConflictingCommandsResponseDtoBatchDto>,
): IGetRequestedCommands {
  const api = useApi();
  const { selectedSite, requestsFilters } = useAppContext();
  const siteFilter = useSiteFilter();

  const sortState = useState<SortSpecificationDto[]>([new SortSpecificationDto({ field: "receivedDate", sort: "desc" })]);

  const body = new GetConflictingCommandsRequestDto();

  const filterSpecifications = [
    ...toFlatArray(requestsFilters[0]),
    siteFilter,
  ].filter((x) => x) as FilterSpecificationDto[];

  body.filterSpecifications = filterSpecifications;
  body.pageSize = 50;
  body.sortSpecifications = sortState[0];

  const query = useInfiniteQuery<ConflictingCommandsResponseDtoBatchDto, ApiException, InfiniteData<ConflictingCommandsResponseDtoBatchDto, number>, QueryKey, number>({
    queryKey: [
      "requestedCommands",
      type,
      sortState[0],
      JSON.stringify(filterSpecifications),
      selectedSite,
    ],
    queryFn: ({ pageParam }) => {
      body.page = pageParam;
      return api.getRequestedCommands(body);
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
