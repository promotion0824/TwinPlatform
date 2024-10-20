import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { ApiException, SiteDto } from "../../services/Clients";
import useApi from "../../hooks/useApi";
import { ComboboxItem } from "@mantine/core";
import useAuthorization from "../../hooks/useAuthorization";

export default function useGetSites(
  options?: Omit<UseQueryOptions<SiteDto[], ApiException, any>, "queryKey" | "queryFn">
) {
  const api = useApi();
  const { hasCanViewRequestsCommandsPermission } = useAuthorization();

  return useQuery<SiteDto[], ApiException, SiteDto[] | ComboboxItem[]>({
    queryKey: ["sites"],
    queryFn: () => api.getAllSites(),
    ...options,
    enabled: hasCanViewRequestsCommandsPermission
  });
}
