import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { TwinId } from "../../../../types/TwinId";
import useApi from "../../../hooks/useApi";
import { ApiException, ConflictingCommandsResponseDto } from "../../../services/Clients";

export default function useGetRequestedCommandsByTwinId(
  twinId?: TwinId,
  options?: Omit<UseQueryOptions<ConflictingCommandsResponseDto, ApiException>, "queryKey" | "queryFn">
) {
  const api = useApi();

  return useQuery<ConflictingCommandsResponseDto, ApiException>({
    queryKey: ["RequestedCommandsByTwinId", twinId],
    queryFn: () => api.getRequestedCommandsByTwinid(twinId!.connectorId!, twinId!.twinId!),
    ...options,
    enabled: !!twinId?.connectorId && !!twinId?.twinId,
  });
}
