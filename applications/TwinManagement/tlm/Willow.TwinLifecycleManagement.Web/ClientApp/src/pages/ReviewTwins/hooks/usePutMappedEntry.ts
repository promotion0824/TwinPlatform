import { useMutation, UseMutationOptions, UseMutationResult } from 'react-query';
import useApi from '../../../hooks/useApi';
import { UpdateMappedEntry } from '../../../services/Clients';

export type IPutMappedEntry = UseMutationResult<any, any, UpdateMappedEntry, any>;
export default function usePutMappedEntry(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();

  return useMutation((mappedEntry: UpdateMappedEntry) => api.putMappedEntry(mappedEntry), options);
}
