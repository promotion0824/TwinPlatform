import { AppPermissions } from './../AppPermissions';
import { AuthHandler } from './../components/AuthHandler';
import useApi from './../hooks/useApi';
import { BasePageInformation } from './../types/BasePageInformation';
import { FileLoadInformation } from './../types/FileLoadInformation';
import { BaseFileUploadPage } from './BaseFileUploadPage';

const ImportTwinsPage = () => {
  const api = useApi('multipart/form-data');

  const pageAction = (info: FileLoadInformation) => {
    return api.importTwins(
      info.FormFiles,
      info.SiteId,
      info.IncludeRelationships,
      info.IncludeTwinProperties,
      info.UserData
    );
  };

  const pageInformation: BasePageInformation = {
    Action: pageAction,
    Type: 'Import',
    Entity: 'Twins',
  };

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanImportTwins]} noAccessAlert={true}>
      {BaseFileUploadPage(pageInformation)}
    </AuthHandler>
  );
};

export default ImportTwinsPage;
