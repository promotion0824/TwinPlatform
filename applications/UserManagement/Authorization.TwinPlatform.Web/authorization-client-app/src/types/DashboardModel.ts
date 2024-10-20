import { PermissionModel } from "./PermissionModel";

export class DashboardModel {

  Users: number;
  Groups: number;
  Roles: number;
  Permissions: number;
  UserAssignments: number;
  GroupAssignments: number;

  constructor() {
    this.Users = 0;
    this.Groups = 0;
    this.Roles = 0;
    this.Permissions = 0;
    this.UserAssignments = 0;
    this.GroupAssignments = 0;
  }

  static AppByPermissions(permissionData: PermissionModel[]) {
    return permissionData ? permissionData.reduce((groupedByExtension: { [key: string]: PermissionModel[] }, row) => {
      const { application } = row;
      groupedByExtension[application.name] = groupedByExtension[application.name] ?? [];
      groupedByExtension[application.name].push(row);
      return groupedByExtension;
    }, {}): [];
  }
}
