import { useQuery, UseQueryOptions } from 'react-query'
/* eslint-disable-next-line */
import {
  EmbedUrlsResponse,
  getEmbedUrls,
  GetEmbedUrlsParams,
} from '../../services/KpiDashboard/EmbedUrlsService'

export default function useGetEmbedUrls(
  params: GetEmbedUrlsParams,
  options?: UseQueryOptions<EmbedUrlsResponse>
) {
  return useQuery(['embedUrls', params], () => getEmbedUrls(params), options)
}
