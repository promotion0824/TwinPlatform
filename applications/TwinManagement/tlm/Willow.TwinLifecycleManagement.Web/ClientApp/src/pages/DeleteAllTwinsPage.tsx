import { AppPermissions } from '../AppPermissions';
import { AuthHandler } from '../components/AuthHandler';
import useApi from '../hooks/useApi';
import useUserInfo from '../hooks/useUserInfo';
import { BasePageInformation } from '../types/BasePageInformation';
import { BaseDeleteAllPage } from './BaseDeleteAllPage';

const DeleteAllTwinsPage = () => {
  const api = useApi();
  const userInfo = useUserInfo();

  const pageAction = (userData: string, deleteOnlyRelationships: boolean) => {
    return api.twinsDELETE(userInfo.userEmail, userData, deleteOnlyRelationships);
  };

  const pageInformation: BasePageInformation = {
    Action: pageAction,
    Type: 'Delete',
    Entity: 'Twins',
  };

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanDeleteAllTwins]} noAccessAlert>
      {BaseDeleteAllPage(pageInformation)}
    </AuthHandler>
  );
};

export default DeleteAllTwinsPage;
