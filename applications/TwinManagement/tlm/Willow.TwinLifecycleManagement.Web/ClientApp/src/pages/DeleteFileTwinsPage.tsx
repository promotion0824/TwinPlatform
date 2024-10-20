import { AppPermissions } from './../AppPermissions';
import { AuthHandler } from './../components/AuthHandler';
import useApi from './../hooks/useApi';
import { BasePageInformation } from './../types/BasePageInformation';
import { FileLoadInformation } from './../types/FileLoadInformation';
import { BaseFileUploadPage } from './BaseFileUploadPage';

const DeleteFileTwinsPage = () => {
  const api = useApi('multipart/form-data');

  const pageAction = (info: FileLoadInformation) => {
    return api.twinsOrRelationshipsBasedOnFile(info.FormFiles, info.UserData, info.DeleteOnlyRelationships);
  };

  const pageInformation: BasePageInformation = {
    Action: pageAction,
    Type: 'Delete',
    Entity: 'Twins',
  };

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanDeleteTwinsorRelationshipByFile]} noAccessAlert={true}>
      {BaseFileUploadPage(pageInformation)}
    </AuthHandler>
  );
};

export default DeleteFileTwinsPage;
