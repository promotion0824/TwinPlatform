import { Guid } from "./Guid";
import * as Yup from 'yup';
import { PermissionModel } from "./PermissionModel";

export const RoleFieldNames = {
  id: { field: "id", label: "Id" },
  name: { field: "name", label: "Name" },
  description: { field: "description", label: "Description" }
}

export type RoleType = {
    id: string,
    name: string,
    description:string,
    permissions: PermissionModel[]
};

export class RoleModel implements RoleType {
    id: string = Guid.Empty.ToString();
    name: string = '';
    description: string = '';
    permissions: PermissionModel[] = [];


    static MapModel(apiRow: any): RoleModel {
        let newRow = new RoleModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }

    static validationSchema = Yup.object().shape({
      name: Yup.string().required('Permission name is required').max(100,'Name cannot be greater than 100 characters in length.'),
      description: Yup.string().max(1000,'Description cannot be greater than 1000 characters in length.')
    });
}
