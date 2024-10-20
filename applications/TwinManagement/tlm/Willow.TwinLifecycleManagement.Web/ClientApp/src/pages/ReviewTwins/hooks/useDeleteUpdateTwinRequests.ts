import { useMutation, UseMutationOptions } from 'react-query';
import useApi from '../../../hooks/useApi';

export default function useDeleteUpdateTwinRequests(selectAll: boolean, options?: UseMutationOptions<any, any, any>) {
  const api = useApi();

  return useMutation(
    ({ ids }: { ids: string[] }) => (selectAll ? api.deleteAllUpdateTwinRequests() : api.deleteUpdateTwinRequests(ids)),
    options
  );
}
