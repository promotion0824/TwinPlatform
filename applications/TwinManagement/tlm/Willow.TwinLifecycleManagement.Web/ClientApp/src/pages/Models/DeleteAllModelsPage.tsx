import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../components/AuthHandler';
import useApi from '../../hooks/useApi';
import useUserInfo from '../../hooks/useUserInfo';
import { BasePageInformation } from '../../types/BasePageInformation';
import { BaseDeleteAllPage } from '../BaseDeleteAllPage';

const DeleteAllModelsPage = () => {
  const api = useApi();
  const userInfo = useUserInfo();

  const pageAction = (userData: string) => {
    return api.modelsDELETE(userInfo.userEmail, userData, false);
  };

  const pageInformation: BasePageInformation = {
    Action: pageAction,
    Type: 'Delete',
    Entity: 'Models',
  };

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanDeleteModels]} noAccessAlert={true}>
      {BaseDeleteAllPage(pageInformation)}
    </AuthHandler>
  );
};

export default DeleteAllModelsPage;
