export type UseInfiniteQueryOptions<TData> = Omit<UndefinedInitialDataInfiniteOptions<TData, ApiException, InfiniteData<TData, number>, QueryKey, number>, "queryFn" | "queryKey">;
