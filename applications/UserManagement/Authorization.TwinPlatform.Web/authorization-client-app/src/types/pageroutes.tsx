import { IconName } from '@willowinc/ui';
import { AppPermissions } from '../AppPermissions';

export interface IPageRoutes {
  route: {
    path: string;
    title: string;
    icon: IconName;
    permission: string[];
  }[]
}

const pageroutes: IPageRoutes['route'] = [
  { path: "", title: "Home", icon: "home", permission: [] },
  { path: "users", title: "Users", icon: "person", permission: [AppPermissions.CanReadUser] },
  { path: "groups", title: "Groups", icon: "group", permission: [AppPermissions.CanReadGroup] },
  { path: "roles", title: "Roles", icon: "badge", permission: [AppPermissions.CanReadRole] },
  { path: "permissions", title: "Permissions", icon: "key", permission: [AppPermissions.CanReadPermission] },
  { path: "applications", title: "Applications", icon: "apps", permission: [AppPermissions.CanReadApplication] },
  { path: "assignments", title: "Assignments", icon: "assignment", permission: [AppPermissions.CanReadAssignment] },
  { path: "about", title: "About", icon: "info", permission: [] },
  { path: "admin", title: "Admin", icon: "shield", permission: [AppPermissions.CanImportData, AppPermissions.CanExportData] }
];

export default pageroutes;
