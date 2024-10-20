import { useQuery, UseQueryOptions } from 'react-query';
import { MtiAsyncJob, ApiException } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetMtiJobs(options?: UseQueryOptions<MtiAsyncJob[], ApiException>) {
  const api = useApi();

  return useQuery<MtiAsyncJob[], ApiException>(['getMtiAsyncJobs'], () => api.findMtiAsyncJob(undefined, undefined), {
    refetchInterval: 1000 * 10, // refetch every 10 seconds
  });
}
