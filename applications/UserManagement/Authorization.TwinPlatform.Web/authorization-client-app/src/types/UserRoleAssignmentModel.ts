import { Guid } from "./Guid";
import * as Yup from 'yup';
import { UserType } from "./UserModel";
import { RoleType } from "./RoleModel";

export type UserRoleAssignmentType = {
    id: string,
    user: UserType | null,
    role: RoleType | null,
    expression: string,
    condition: string
};

export class UserRoleAssignmentModel implements UserRoleAssignmentType {
    id: string = Guid.Empty.ToString();
    user: UserType | null = null;
    role: RoleType | null = null;
    expression: string = '';
    condition: string = '';


    static MapModel(apiRow: any): UserRoleAssignmentModel {
        let newRow = new UserRoleAssignmentModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }

    static validationSchema = Yup.object().shape({
        user: Yup.object().nullable().required('Please select a user'),
        role: Yup.object().nullable().required('Please select a role'),
    });
}
