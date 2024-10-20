import { Alert, AlertProps, Button, Link, Snackbar } from '@mui/material';
import { endpoints } from '../config';
import { useState } from 'react';
import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, AppVersion } from '../services/Clients';
import useApi from '../hooks/useApi';

function useGetVersion(options?: UseQueryOptions<AppVersion, ApiException>) {
  const api = useApi();
  return useQuery<AppVersion, ApiException>(['version'], () => api.getTlmAndDependenciesVersions(), {
    ...options,
    staleTime: Infinity,
  });
}

const AboutPage = () => {
  const { data } = useGetVersion();
  const [snackbar, setSnackbar] = useState<Pick<AlertProps, 'children' | 'severity'> | null>(null);
  const handleCloseSnackbar = () => setSnackbar(null);

  const clearCache = () => {
    localStorage.clear();
    setSnackbar({ children: 'Cache cleared successfully', severity: 'success' });
  };

  return (
    <div>
      <div style={{ marginTop: 20 }}>
        <Button variant="contained" onClick={clearCache}>
          Clear Cache
        </Button>
        {!!snackbar && (
          <Snackbar
            sx={{ top: '90px !important' }}
            open
            anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
            onClose={handleCloseSnackbar}
            autoHideDuration={6000}
          >
            <Alert {...snackbar} onClose={handleCloseSnackbar} variant="filled" />
          </Snackbar>
        )}
      </div>
      <div style={{ marginTop: 10 }}>TLM Version: {data?.tlmAssemblyVersion}</div>
      <div>ADT_API Version: {data?.adtApiVersion}</div>
      <br />
      <p>
        See <Link href={endpoints.userGuideLink}>User Guide</Link> for more information on how to use the tool.
      </p>
      <p>
        Reach out to <Link href={endpoints.supportLink}>support@willowinc.com</Link> in case of any issues.
      </p>
      <p>
        Thank you,
        <br />
        TLM Team
      </p>
    </div>
  );
};

export default AboutPage;
