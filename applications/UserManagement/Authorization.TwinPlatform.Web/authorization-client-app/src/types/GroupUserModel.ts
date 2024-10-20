import { Guid } from "./Guid";
import * as Yup from 'yup';
import { UserModel } from "./UserModel";


export type GroupUserType = {
    id: string,
    groupId: string,
    user: UserModel | null,
};

export class GroupUserModel implements GroupUserType {
    id: string = Guid.Empty.ToString();
    groupId: string = '';
    user: UserModel | null = null;

    static MapModel(apiRow: any): GroupUserModel {
        let newRow = new GroupUserModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }

    static validationSchema = Yup.object().shape({
        user: Yup.object().nullable().required('Please select a user'),
    });
}
