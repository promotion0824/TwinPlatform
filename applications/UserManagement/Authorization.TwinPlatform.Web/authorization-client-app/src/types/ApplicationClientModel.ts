import * as Yup from 'yup';
import { Guid } from "./Guid";
import { ApplicationModel } from './ApplicationModel';

export class ApplicationClientModel {
  id: string = Guid.Empty.ToString();
  name: string = '';
  description: string = '';
  clientId: string = Guid.Empty.ToString();
  application: ApplicationModel = new ApplicationModel();

  static MapModel(apiRow: any): ApplicationClientModel {
    let newRow = new ApplicationClientModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }

  static validationSchema = Yup.object().shape({
    name: Yup.string().required('Client name is required.').max(100, 'Name cannot be greater than 100 characters in length.'),
    description: Yup.string().required('Description is required.').max(200, 'Description cannot be greater than 500 characters in length.'),
  });

}
