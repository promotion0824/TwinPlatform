
export class ClientAppPasswordModel {
  name: string = '';
  secretText: string = '';
  startTime: Date = new Date();
  endTime: Date = new Date();

  static MapModel(apiRow: any): ClientAppPasswordModel {
    let newRow = new ClientAppPasswordModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }
}

export type SecretCredentials = {
  [key: string]: ClientAppPasswordModel;
}
