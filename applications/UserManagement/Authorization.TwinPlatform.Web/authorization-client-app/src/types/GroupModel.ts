import { GroupType } from "./GroupTypeModel";
import { Guid } from "./Guid";
import * as Yup from 'yup';

export type Group = {
    id: string,
    name: string,
    groupTypeId: string,
    groupType:GroupType | null
    users: any[]
};

export class GroupModel implements Group {
    id: string=Guid.Empty.ToString();
    name: string = '';
    groupTypeId: string = Guid.Empty.ToString();
    groupType: GroupType | null = null;
    users: any[] = [];


    static MapModel(apiRow: any): GroupModel {
        let newRow = new GroupModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }

    static validationSchema = Yup.object().shape({
        name: Yup.string().required('Group name is required.').max(100, 'Name cannot be greater than 100 characters in length.'),
        groupType: Yup.object().nullable().required('Please select a group type.'),
    });
}
