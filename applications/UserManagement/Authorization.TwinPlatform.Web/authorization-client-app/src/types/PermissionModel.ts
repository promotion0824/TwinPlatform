import { ApplicationModel } from "./ApplicationModel";
import { Guid } from "./Guid";
import * as Yup from 'yup';

export const PermissionFieldNames = {
  id: { field: "id", label: "Id" },
  name: { field: "name", label: "Name" },
  applicationName: { field: "application.Name", label: "Application" },
  description: { field: "description", label: "Description" },
  applicationId : { field: "applicationId", label: "ApplicationId" },
}

export type PermissionType = {
  id: string,
  name: string,
  application: ApplicationModel,
  description: string

}

export class PermissionModel implements PermissionType {
  id: string = Guid.Empty.ToString();
  name: string = '';
  application: ApplicationModel = new ApplicationModel();
  description: string = '';

  static MapModel(apiRow: any): PermissionModel {
    let newRow = new PermissionModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }

  static validationSchema = Yup.object().shape({
    name: Yup.string().required('Permission name is required')
      .min(2, 'Permission name must be minimum of 2 characters')
      .max(100, 'Name cannot be greater than 100 characters in length.'),
    application: Yup.object().nullable().required('Please select a application.'),
    description: Yup.string().max(200, 'Description cannot be greater than 200 characters in length.')
  });

  static GetFullName(model: PermissionModel): string {
    return model.name + ' (' + model.application?.name + ')';
  }
}
