import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { ApiException, FilterSpecificationDto, GetRequestedCommandsCountDto, RequestedCommandStatus } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";
import { useAppContext } from "../../../providers/AppContextProvider";
import { useSiteFilter } from "../../../hooks/useSiteFilter";
import { toFlatArray } from "../../../utils/toArray";

export const useGetRequestedCommandsCount = (options?: Omit<UseQueryOptions<number, ApiException>, "queryKey" | "queryFn">) => {
  const api = useApi();

  const { requestsFilters } = useAppContext();
  const siteFilter = useSiteFilter();

  const body = new GetRequestedCommandsCountDto();

  const filterSpecifications = [
    ...toFlatArray<FilterSpecificationDto>(requestsFilters[0]),
    siteFilter,
  ].filter((x) => x) as FilterSpecificationDto[];

  body.filterSpecifications = filterSpecifications;
  body.status = RequestedCommandStatus.Pending; // filter by pending requests commands

  return useQuery<number, ApiException>({
    queryKey: ["requestedCommandsCount", JSON.stringify(filterSpecifications)],
    queryFn: () => api.getRequestedCommandsCount(body),
    ...options,
  });
}
