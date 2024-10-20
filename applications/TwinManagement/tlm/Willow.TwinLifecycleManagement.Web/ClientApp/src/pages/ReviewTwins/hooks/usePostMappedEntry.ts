import { useMutation, UseMutationOptions } from 'react-query';
import useApi from '../../../hooks/useApi';
import { CreateMappedEntry } from '../../../services/Clients';

export default function usePostMappedEntry(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();

  return useMutation((mappedEntry: CreateMappedEntry) => api.createMappedEntry(mappedEntry), options);
}
