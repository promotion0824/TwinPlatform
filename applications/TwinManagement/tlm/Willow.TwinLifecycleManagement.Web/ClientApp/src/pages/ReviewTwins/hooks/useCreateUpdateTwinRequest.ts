import { useMutation, UseMutationOptions } from 'react-query';
import useApi from '../../../hooks/useApi';
import { JsonPatchOperation, MtiAsyncJobRequest } from '../../../services/Clients';

// This hook is used for testing purposes. It will make request to create a new update twin request.
export default function useCreateUpdateTwinRequest(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();

  return useMutation(
    ({ willowTwinId, jsonPatchOperations }: { willowTwinId: string; jsonPatchOperations: JsonPatchOperation[] }) =>
      api.createUpdateTwinRequests(willowTwinId, jsonPatchOperations),
    options
  );
}

/**
 *  example usage:
const { mutate } = useCreateUpdateTwinRequest();

function handleCreateTwinRequest() {
  var num = new JsonPatchOperation({ op: OperationType.Add, path: '/num', value: 1 });
  var string = new JsonPatchOperation({ op: OperationType.Add, path: '/string', value: 'hello' });
  var bool = new JsonPatchOperation({ op: OperationType.Add, path: '/bool', value: true });
  var remove = new JsonPatchOperation({ op: OperationType.Remove, path: '/num' });
  var replace = new JsonPatchOperation({ op: OperationType.Replace, path: '/string', value: 'world' });

  mutate({
    willowTwinId: '123',
    jsonPatchOperations: [num, string, bool, remove, replace],
  });
}

 * 
 */

export function useMtiJob(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();

  return useMutation(({ request }: { request: MtiAsyncJobRequest }) => api.createMtiAsyncJob(request), options);
}
