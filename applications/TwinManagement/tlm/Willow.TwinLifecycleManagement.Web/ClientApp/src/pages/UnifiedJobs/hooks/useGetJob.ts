import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, JobsEntry } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetJob(jobId: string, options?: UseQueryOptions<JobsEntry, ApiException>) {
  const api = useApi();

  return useQuery<JobsEntry, ApiException>(['getUnifiedJob', jobId], () => api.getJob(jobId), options);
}
