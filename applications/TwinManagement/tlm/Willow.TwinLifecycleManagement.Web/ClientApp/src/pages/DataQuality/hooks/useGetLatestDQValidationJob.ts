import { useQuery, UseQueryOptions } from 'react-query';
import { ITwinsValidationJob, ApiException, AsyncJobStatus } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

interface IGetLatestDQValidationJobParams {
  status?: AsyncJobStatus;
}

export default function useGetLatestDQValidationJob(
  params: IGetLatestDQValidationJobParams,
  options?: UseQueryOptions<ITwinsValidationJob, ApiException>
) {
  const api = useApi();

  const { status } = params;

  return useQuery<ITwinsValidationJob, ApiException>(
    ['getLatestDQValidationJob', status],
    () => api.getLatestDQValidationJob(status),
    options
  );
}
