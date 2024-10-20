import { useState } from "react";
import { InfiniteData, QueryKey, UndefinedInitialDataInfiniteOptions, useInfiniteQuery, UseInfiniteQueryResult } from "@tanstack/react-query";
import { ApiException, GetResolvedCommandsDto, SortSpecificationDto, FilterSpecificationDto, ResolvedCommandResponseDtoBatchDto } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";
import { useAppContext } from "../../../providers/AppContextProvider";
import { UseState } from "../../../../types/UseState";
import { getFilterSpecification } from "../../../utils/getFilterSpecification";
import { UseInfiniteQueryOptions } from "../../../../types/UseInfiniteQueryOptions";
import { toFlatArray } from "../../../utils/toArray";
import { useSiteFilter } from "../../../hooks/useSiteFilter";

export interface IGetResolvedCommands {
  query: UseInfiniteQueryResult<InfiniteData<ResolvedCommandResponseDtoBatchDto, number>, ApiException>;
  sortState: UseState<SortSpecificationDto[]>;
}

export default function useGetResolvedCommands(
  type: "commands" | "pastCommands",
  defaultFilters: FilterSpecificationDto[] = [],
  options?: UseInfiniteQueryOptions<ResolvedCommandResponseDtoBatchDto>,
) {
  const api = useApi();
  const { selectedSite, commandsFilters } = useAppContext();
  const siteFilter = useSiteFilter();

  const sortState = useState<SortSpecificationDto[]>([new SortSpecificationDto({field: "startTime", sort: "asc"})]);

  const body = new GetResolvedCommandsDto();

  const filterSpecifications = [
    ...toFlatArray(commandsFilters[0]),
    ...defaultFilters,
    siteFilter,
  ].filter((x) => x) as FilterSpecificationDto[];

  body.filterSpecifications = filterSpecifications;
  body.pageSize = 100;
  body.sortSpecifications = sortState[0];

  const key: QueryKey = [
    "ResolvedCommands",
    type,
    sortState[0],
    JSON.stringify(filterSpecifications),
    selectedSite,
  ];
  const { enabled, ...otherOptions } = options ?? {};

  const query = useInfiniteQuery<ResolvedCommandResponseDtoBatchDto, ApiException, InfiniteData<ResolvedCommandResponseDtoBatchDto, number>, QueryKey, number>({
    queryKey: key,
    queryFn: ({ pageParam }) => {
      body.page = pageParam;
      return api.getResolvedCommands(body);
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages, lastPageParam) => {
      if (allPages.flatMap(a => a.items ?? []).length < lastPage.total!) {
        return lastPageParam + 1;
      }
    },
    ...otherOptions,
    enabled:
      enabled &&
      !!selectedSite
  });

  return {
    query,
    key,
    sortState,
  };
}
