import { useQuery, UseQueryOptions } from 'react-query';
import { ITwinsValidationJob, ApiException } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetDQValidationJobs(options?: UseQueryOptions<ITwinsValidationJob[], ApiException>) {
  const api = useApi();

  const { id, userId, status, from, to, fullDetails } = {
    id: undefined,
    userId: undefined,
    status: undefined,
    from: undefined,
    to: undefined,
    fullDetails: true,
  };

  return useQuery<ITwinsValidationJob[], ApiException>(
    ['getDQValidationJobs'],
    () => api.getDQValidationJobs(id, userId, status, from, to, fullDetails),
    options
  );
}
