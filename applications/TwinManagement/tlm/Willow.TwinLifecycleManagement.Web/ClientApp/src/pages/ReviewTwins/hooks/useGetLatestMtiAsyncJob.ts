import { useQuery, UseQueryOptions } from 'react-query';
import { MtiAsyncJob, ApiException, AsyncJobStatus } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetLatestMtiAsyncJob(
  status?: AsyncJobStatus,
  options?: UseQueryOptions<MtiAsyncJob, ApiException>
) {
  const api = useApi();

  return useQuery<MtiAsyncJob, ApiException>(
    ['GetLatestMtiAsyncJob', status],
    () => api.getLatestMtiAsyncJob(status),
    options
  );
}
