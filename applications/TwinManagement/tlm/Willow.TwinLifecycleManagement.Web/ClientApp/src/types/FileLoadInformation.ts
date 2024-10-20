import { FileParameter } from '../services/Clients';

export type FileLoadInformation = {
  SiteId: string;

  Type: string;

  UserData: string;

  IncludeRelationships: boolean;

  IncludeTwinProperties: boolean;

  FormFiles: FileParameter[];

  DeleteOnlyRelationships: boolean;
};
