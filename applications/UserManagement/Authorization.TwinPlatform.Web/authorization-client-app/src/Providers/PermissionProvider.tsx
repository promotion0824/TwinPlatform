import { ReactNode, useContext, useEffect, useState } from "react"
import { useLoading } from "../Hooks/useLoading";
import { AuthorizationClient } from "../Services/AuthClient";
import { AuthorizationModel } from "../types/AuthorizationModel";
import { PermissionContext } from "../types/PermissionContext"

const PermissionProvider = ({ children }: { children: ReactNode }) => {

  const [authData, setAuthData] = useState(new AuthorizationModel());
  const loader = useLoading();

  useEffect(() => {
    const RefetchAuthData = async () => {
      try {

        loader(true, 'Checking your access. Please wait.');
        console.log('AuthHandler calling backend ');
        const response = await AuthorizationClient.GetAuthorizationData();
        setAuthData(response);

      } catch (e) {
        console.error(e);
      } finally {
        loader(false);
      }
    }
    RefetchAuthData();
  }, []);


  return (
    <PermissionContext.Provider value={authData}>
      {children}
    </PermissionContext.Provider>
  );
}

export const useAuth = () => useContext(PermissionContext);

export default PermissionProvider;

