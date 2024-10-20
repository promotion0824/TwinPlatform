import AuthRoutes from './Components/AuthRoutes';
import RequestInterceptor from './Providers/RequestInterceptor';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const App = () => {

  return (
    <RequestInterceptor>
      <QueryClientProvider client={queryClient}>
        <AuthRoutes />
      </QueryClientProvider>
    </RequestInterceptor>
  );
}

export default App;
