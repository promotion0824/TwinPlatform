import { UseQueryOptions as UseQueryOptionsRq, QueryKey } from "react-query";

export type UseQueryOptions<TData> = Omit<UseQueryOptionsRq<TData, ApiException, QueryKey>, "queryFn" | "queryKey">;
