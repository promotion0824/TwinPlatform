import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { ApiException, ResolvedCommandResponseDto } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";

export default function useGetResolvedCommandById(
  id?: string,
  options?: Omit<UseQueryOptions<ResolvedCommandResponseDto, ApiException>, "queryKey" | "queryFn">
) {
  const api = useApi();

  return useQuery<ResolvedCommandResponseDto, ApiException>({
    queryKey: ["ResolvedCommandsById", id],
    queryFn: () => api.getResolvedCommandById(id!),
    ...options,
    enabled: !!id,
  });
}
