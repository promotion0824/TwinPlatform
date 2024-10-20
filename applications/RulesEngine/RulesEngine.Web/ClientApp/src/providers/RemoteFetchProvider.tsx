import { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryCache = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
      // Removed staleTime: default queryclient config causes React app to re-render all children in app.  QueryClient default config mods have consequences
      // staleTime: 30000, 
    },
  },
});

const ReactFetchProvider = ({ children }: { children: ReactNode; }) => {
  return (
    <QueryClientProvider client={queryCache}>{children}</QueryClientProvider>
  );
};

export default ReactFetchProvider;
