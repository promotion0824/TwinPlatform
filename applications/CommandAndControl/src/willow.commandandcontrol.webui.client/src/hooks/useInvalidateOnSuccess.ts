import { QueryKey, UseMutationOptions, useQueryClient } from "@tanstack/react-query"

export const useInvalidateOnSuccess = () => {
  var queryClient = useQueryClient();

  return (key: QueryKey): UseMutationOptions<any, any, any, any> => ({
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: key });
    }
  });
}
