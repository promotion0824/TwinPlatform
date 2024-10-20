import { useMutation, UseMutationOptions } from 'react-query';
import useApi from '../../../hooks/useApi';
import useUserInfo from '../../../hooks/useUserInfo';

export type ValidateTwinsMutateParams = {
  modelIds: string[];
  locationId?: string;
  exactModelMatch?: boolean;
  isIncrementalScan?: boolean;
};

export default function useMutateDQTwinValidation(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();
  const userInfo = useUserInfo();
  return useMutation(
    ({ modelIds = [], locationId, exactModelMatch = false, isIncrementalScan }: ValidateTwinsMutateParams) =>
      api.validateTwins(userInfo.userEmail, modelIds, locationId, isIncrementalScan, exactModelMatch, undefined),
    options
  );
}
