import { useMutation, UseMutationOptions, UseMutationResult } from "@tanstack/react-query";
import useApi from "../../../hooks/useApi";
import { UpdateRequestedCommandsStatusDto } from "../../../services/Clients";

export type IUpdateRequestedCommandsStatusDto = UseMutationResult<
  any,
  any,
  Variables,
  any
>;
export default function usePostRequestedCommandsStatus(options?: UseMutationOptions<any, any, Variables, any>) {
  const api = useApi();

  return useMutation<any, any, Variables, any>({
    mutationFn: ({ updateRequestedCommandsStatus, }: Variables) => api.updateRequestedCommandsStatus(updateRequestedCommandsStatus),
    ...options
});
}

interface Variables {
  updateRequestedCommandsStatus: UpdateRequestedCommandsStatusDto;
}
