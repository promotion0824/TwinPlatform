import { useQuery, UseQueryOptions } from 'react-query';
import { IGetRulesResponse, ApiException } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetDQRules(options?: UseQueryOptions<IGetRulesResponse, ApiException>) {
  const api = useApi();

  return useQuery<IGetRulesResponse, ApiException>(['getDQRules'], () => api.getDQRules(), options);
}
