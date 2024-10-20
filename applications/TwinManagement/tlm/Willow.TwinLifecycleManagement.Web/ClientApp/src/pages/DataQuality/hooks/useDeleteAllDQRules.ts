import { useMutation, UseMutationOptions } from 'react-query';
import useApi from '../../../hooks/useApi';

export default function useDeleteAllDQRules(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();
  return useMutation(() => api.deleteAllDQRules(), options);
}
