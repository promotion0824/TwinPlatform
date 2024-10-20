import { ReactNode } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const queryCache = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
    },
  },
});

const ReactFetchProvider = ({ children }: { children: ReactNode }) => {
  return (
    <QueryClientProvider client={queryCache}>{children}</QueryClientProvider>
  );
};

export default ReactFetchProvider;
