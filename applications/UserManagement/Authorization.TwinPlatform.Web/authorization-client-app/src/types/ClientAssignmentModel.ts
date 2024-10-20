import { ApplicationClientModel } from "./ApplicationClientModel";
import { Guid } from "./Guid";
import * as Yup from 'yup';
import { PermissionModel } from "./PermissionModel";

export class ClientAssignmentModel {
  id: string = Guid.Empty.ToString();
  applicationClient: ApplicationClientModel | null = null;
  permissions: PermissionModel[] = [];
  expression: string = '';
  condition: string = '';


  static MapModel(apiRow: any): ClientAssignmentModel {
    let newRow = new ClientAssignmentModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }

  static validationSchema = Yup.object().shape({
    applicationClient: Yup.object().nullable().required('Please select a client for assignment'),
    permissions: Yup.array().min(1, "Please at least one permission for assignment").required(),
    expression: Yup.string().max(1000, 'Expression cannot be greater than 1000 characters in length.'),
    condition: Yup.string().max(400, 'Description cannot be greater than 400 characters in length.'),
  });
}
