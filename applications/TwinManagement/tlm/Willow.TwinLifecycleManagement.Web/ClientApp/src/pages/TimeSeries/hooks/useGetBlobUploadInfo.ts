import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, BlobUploadInfo } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetBlobUploadInfo(
  params: {
    fileNames: string[];
  },
  options?: UseQueryOptions<BlobUploadInfo, ApiException>) {
  const api = useApi();
  const { fileNames } = params;
  const query = useQuery<BlobUploadInfo, ApiException>(['time-series-sas-token', fileNames], () => api.getTimeSeriesBlobUploadInfo(fileNames), {
    ...options,
    enabled: false,
  });

  return query;
}
