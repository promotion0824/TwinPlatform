import { Guid } from "./Guid";
import * as Yup from 'yup';
import { RoleType } from "./RoleModel";
import { Group } from "./GroupModel";

export type GroupRoleAssignmentType = {
    id: string,
    group: Group | null,
    role: RoleType | null,
    expression: string,
    condition: string
};

export class GroupRoleAssignmentModel implements GroupRoleAssignmentType {
    id: string = Guid.Empty.ToString();
    group: Group | null = null;
    role: RoleType | null = null;
    expression: string = '';
    condition: string = '';


    static MapModel(apiRow: any): GroupRoleAssignmentModel {
        let newRow = new GroupRoleAssignmentModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }

    static validationSchema = Yup.object().shape({
        group: Yup.object().nullable().required('Please select a group'),
        role: Yup.object().nullable().required('Please select a role'),
    });
}
