import { PermissionModel } from "./PermissionModel";
import { UserModel } from "./UserModel";


export type ConditionalPermissionType = {
  condition: string,
  permission: PermissionModel
};

export type UserProfileType = {
  user: UserModel,
  permissions: ConditionalPermissionType[] | undefined
};


export class UserProfileModel implements UserProfileType {
  user!: UserModel;
  permissions: ConditionalPermissionType[] | undefined;
  adGroupBasedPermissions: ConditionalPermissionType[] | undefined;
  static MapModel(apiRow: any): UserProfileModel {
    let newRow = new UserProfileModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }
}
