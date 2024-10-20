import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import { ReactNode } from 'react';
import { useAuth, AuthLogic } from '../hooks/useAuth';

export const ShowNoAccessAlert = ({ showAlert }: { showAlert: boolean }) => {
  if (showAlert)
    return (
      <Box>
        <Alert severity="info">You are not allowed to access this resource</Alert>
      </Box>
    );
  else return <></>;
};

export const AuthHandler = ({
  children,
  requiredPermissions,
  noAccessAlert = false,
  authLogic = AuthLogic.All,
}: {
  children: ReactNode;
  requiredPermissions: string[] | null;
  noAccessAlert?: boolean;
  authLogic?: AuthLogic;
}) => {
  const auth = useAuth();
  return (
    <>
      {auth.hasPermission(requiredPermissions, authLogic) ? (
        children
      ) : (
        <ShowNoAccessAlert showAlert={noAccessAlert && !auth.isLoading()}></ShowNoAccessAlert>
      )}
    </>
  );
};
