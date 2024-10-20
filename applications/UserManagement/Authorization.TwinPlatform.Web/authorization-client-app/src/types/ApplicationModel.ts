import { Guid } from "./Guid";

export class ApplicationModel {
  id: string = Guid.Empty.ToString();
  name: string = '';
  description: string = '';
  supportClientAuthentication: boolean = false;

  static MapModel(apiRow: any): ApplicationModel {
    let newRow = new ApplicationModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }
}
