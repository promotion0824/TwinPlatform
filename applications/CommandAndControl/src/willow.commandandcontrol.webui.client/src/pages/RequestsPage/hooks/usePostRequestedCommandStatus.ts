import { useMutation, UseMutationResult } from "@tanstack/react-query";
import useApi from "../../../hooks/useApi";
import { UpdateRequestedCommandStatusDto } from "../../../services/Clients";

export type IUpdateRequestedCommandStatusDto = UseMutationResult<
  any,
  any,
  any,
  any
>;
export default function usePostRequestedCommandStatus(options?: any) {
  const api = useApi();

  return useMutation({
    mutationFn: ({
      id,
      updateRequestedCommandStatus,
    }: {
      id: string;
      updateRequestedCommandStatus: UpdateRequestedCommandStatusDto;
    }) => api.updateRequestedCommandStatus(id, updateRequestedCommandStatus),
    ...options
  });
}
