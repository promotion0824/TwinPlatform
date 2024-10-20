import { useMutation, UseMutationResult } from "@tanstack/react-query";
import useApi from "../../../hooks/useApi";
import { UpdateResolvedCommandStatusDto } from "../../../services/Clients";

export type IUpdateResolvedCommandStatusDto = UseMutationResult<
  any,
  any,
  any,
  any
>;
export default function usePostResolvedCommandStatus(options?: any): IUpdateResolvedCommandStatusDto {
  const api = useApi();

  return useMutation({
    mutationFn: ({
      id,
      updateResolvedCommandStatus,
    }: {
      id: string;
      updateResolvedCommandStatus: UpdateResolvedCommandStatusDto;
    }) => api.updateResolvedCommandStatus(id, updateResolvedCommandStatus),
    ...options
  });
}
