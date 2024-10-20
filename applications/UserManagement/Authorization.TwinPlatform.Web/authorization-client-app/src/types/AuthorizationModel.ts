
export class AuthorizationModel {
  permissions: string[] = [];
  isAdminUser: boolean = false;

  static MapModel(apiRow: any): AuthorizationModel {
    let newRow = new AuthorizationModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }

}
