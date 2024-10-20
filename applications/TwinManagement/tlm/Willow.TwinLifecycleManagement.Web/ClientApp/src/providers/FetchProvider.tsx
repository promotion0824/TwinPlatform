import { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';

const TlmFetchProvider = ({ children }: { children: ReactNode }) => {
  const queryCache = new QueryClient({
    defaultOptions: {
      queries: {
        refetchOnWindowFocus: false,
        retry: false,
      },
    },
  });
  return <QueryClientProvider client={queryCache}>{children}</QueryClientProvider>;
};

export default TlmFetchProvider;
