import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetJobType(options?: UseQueryOptions<string[], ApiException>) {
  const api = useApi();

  return useQuery<string[], ApiException>(['getUnifiedJobType'], () => api.getJobtypes());
}
