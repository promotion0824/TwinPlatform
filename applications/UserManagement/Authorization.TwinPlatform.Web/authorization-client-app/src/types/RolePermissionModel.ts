import { Guid } from "./Guid";
import * as Yup from 'yup';
import { PermissionModel } from "./PermissionModel";


export type RolePermissionType = {
    id: string,
    roleId:string,
    permission: PermissionModel |null,
};

export class RolePermissionModel implements RolePermissionType {
    id: string = Guid.Empty.ToString();
    roleId: string = '';
    permission: PermissionModel | null = null;

    static MapModel(apiRow: any): RolePermissionModel {
        let newRow = new RolePermissionModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }

    static validationSchema = Yup.object().shape({
        permission: Yup.object().nullable().required('Please select a permission'),
    });
}
