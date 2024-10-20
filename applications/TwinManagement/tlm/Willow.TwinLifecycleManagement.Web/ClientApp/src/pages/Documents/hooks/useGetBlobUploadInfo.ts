import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, BlobUploadInfo } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';
import { useState } from 'react';

export default function useGetBlobUploadInfo(
  params: {
    fileNames: string[];
  },
  options?: UseQueryOptions<BlobUploadInfo, ApiException>) {
  const api = useApi();
  const { fileNames } = params;
  const query = useQuery<BlobUploadInfo, ApiException>(['sas-token', fileNames], () => api.getBlobUploadInfo(fileNames), {
    ...options,
    enabled: false,
  });

  return { query };
}
