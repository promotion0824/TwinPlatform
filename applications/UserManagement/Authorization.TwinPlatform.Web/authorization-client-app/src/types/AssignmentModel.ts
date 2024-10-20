import { Guid } from "./Guid";
import * as Yup from 'yup';
import { UserType } from "./UserModel";
import { RoleType } from "./RoleModel";
import { Group } from "./GroupModel";
import { ExpressionStatus } from "./ExpressionStatus";

export type UserOrGroupType = 'U' | 'G' | null;

export type AssignmentType = {
    id: string,
    userOrGroup: UserType | Group | null,
    type: UserOrGroupType,
    role: RoleType | null,
    expression: string,
    condition: string,
    conditionExpressionStatus: ExpressionStatus
};

export class AssignmentModel implements AssignmentType {
    id: string = Guid.Empty.ToString();
    userOrGroup: UserType | Group | null = null;
    type: UserOrGroupType = null;
    role: RoleType | null = null;
    expression: string = '';
    condition: string = '';
    conditionExpressionStatus = ExpressionStatus.Unknown;

    static MapModel(apiRow: any,assignmentType: UserOrGroupType): AssignmentModel {
        let newRow = new AssignmentModel();
        Object.assign(newRow, apiRow);
        newRow.type = assignmentType;

        if (newRow.type==='U') {
            newRow.userOrGroup = apiRow['user'];
        } else {
            newRow.userOrGroup = apiRow['group'];
        }
        return newRow;
    }

    static validationSchema = Yup.object().shape({
        userOrGroup: Yup.object().nullable().required('Please select a user or group'),
        role: Yup.object().nullable().required('Please select a role'),
    });
}
