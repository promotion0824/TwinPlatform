import { Guid } from "./Guid";
import * as Yup from 'yup';

export type GroupType = {
    id: string,
    name: string,
};

export class GroupTypeModel implements GroupType {
    id: string=Guid.Empty.ToString();
    name: string='';

    static MapModel(apiRow: any): GroupTypeModel {
        let newRow = new GroupTypeModel();
        Object.assign(newRow, apiRow);
        return newRow;
    }
}
