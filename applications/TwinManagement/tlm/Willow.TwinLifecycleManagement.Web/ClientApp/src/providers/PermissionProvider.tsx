import { ReactNode, useEffect, useState } from 'react';
import { AuthorizationPermissionContext, AuthDataContext } from '../context/AuthorizationPermissionContext';
import useApi from '../hooks/useApi';
import useLoader from '../hooks/useLoader';

const PermissionProvider = ({ children }: { children: ReactNode }) => {
  const [authData, setAuthData] = useState(new AuthDataContext());
  const [showloader, hideLoader] = useLoader();
  const api = useApi();

  useEffect(
    () => {
      const fetchAuthData = async () => {
        try {
          showloader();
          console.info('Calling Authorization API for Permission');
          setAuthData({ ...authData, ...{ isLoading: true } });
          let authResponse = await api.authorization();
          setAuthData({ ...authData, ...{ response: authResponse, isLoading: false } });
          console.info('Successfully retrieved permissions from Authorization API');
        } catch (e) {
          console.error(e);
          setAuthData({ ...authData, ...{ isLoading: false } });
        } finally {
          hideLoader();
        }
      };

      fetchAuthData();
    }, // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  return <AuthorizationPermissionContext.Provider value={authData}>{children}</AuthorizationPermissionContext.Provider>;
};

export default PermissionProvider;
