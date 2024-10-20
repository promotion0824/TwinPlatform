import { AxiosRequestConfig, AxiosResponse } from "axios";
import { getAuthClient } from "../axiosClients";
import { PermissionModel } from "../types/PermissionModel";
import { RoleModel } from "../types/RoleModel";
import { RolePermissionType } from "../types/RolePermissionModel";
import { FilterOptions } from "../types/FilterOptions";
import { UserModel } from "../types/UserModel";
import { GroupModel } from "../types/GroupModel";
import { GroupUserType } from "../types/GroupUserModel";
import { AssignmentModel } from "../types/AssignmentModel";
import { UserRoleAssignmentModel } from "../types/UserRoleAssignmentModel";
import { GroupRoleAssignmentModel } from "../types/GroupRoleAssignmentModel";
import { ConfigModel } from "../types/ConfigModel";
import { AuthorizationModel } from "../types/AuthorizationModel";
import { DashboardModel } from "../types/DashboardModel";
import { GroupTypeModel } from "../types/GroupTypeModel";
import { ILocationTwinModel } from "../types/SelectTreeModel";
import { ApplicationModel } from "../types/ApplicationModel";
import { ApplicationClientModel } from "../types/ApplicationClientModel";
import { ClientAppPasswordModel } from "../types/ClientAppPasswordModel";
import { ClientAssignmentModel } from "../types/ClientAssignmentModel";
import { BatchRequestDto } from "../types/BatchRequestDto";
import { BatchDto } from "../types/BatchDto";

export class DashboardClient {
  static GetAllData = async (): Promise<DashboardModel> => {
    let dashboardData: { data: DashboardModel } = await getAuthClient().get("dashboard");
    return dashboardData.data;
  };
}

export class UserClient {

  static GetAllUsers = async (request?: BatchRequestDto): Promise<BatchDto<UserModel>> => {
    let batch: { data: BatchDto<UserModel> } = await getAuthClient().post("users/batch", JSON.stringify(request));
    return batch.data;
  };

  static GetUsersByGroup = async (groupId: string, request: BatchRequestDto, getOnlyNonMembers: boolean): Promise<BatchDto<UserModel>> => {
    let batch: { data: BatchDto<UserModel> } = await getAuthClient().post("users/batch/group?groupId=" + groupId + "&getOnlyNonMembers=" + getOnlyNonMembers, JSON.stringify(request));
    return batch.data;
  };

  static GetAdminUsers = async (): Promise<UserModel[]> => {
    let users: { data: [] } = await getAuthClient().get("users/admins");
    return users.data.map((x) => UserModel.MapModel(x));
  };

  static GetUserByEmail = async (email: string, includePermission: boolean = false, includeADGroupPermissions: boolean = false): Promise<AxiosResponse> => {
    return getAuthClient().get(`users/${encodeURIComponent(email)}?includePermissions=${includePermission}&includeADGroupPermissions=${includeADGroupPermissions}`);
  };

  static AddUser = async (data: UserModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().post("users", JSON.stringify(data));
  };

  static UpdateUser = async (data: UserModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().put("users", JSON.stringify(data));
  };

  static DeleteUser = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("users/" + id);
  };

  static PrepareModel(data: UserModel) {
    data.firstName = data.firstName?.trim();
    data.lastName = data.lastName?.trim();
    data.email = data.email?.trim();
    return data;
  }
}

export class RoleClient {

  static GetAllRoles = async (request?: BatchRequestDto): Promise<BatchDto<RoleModel>> => {
    let batch: { data: BatchDto<RoleModel> } = await getAuthClient().post("roles/batch", JSON.stringify(request));
    return batch.data;
  };

  static AddRole = async (data: RoleModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().post("roles", JSON.stringify(data));
  };

  static UpdateRole = async (data: RoleModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().put("roles", JSON.stringify(data));
  };

  static DeleteRole = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("roles/" + id);
  };

  static GetRoleByName = async (name: string): Promise<RoleModel> => {
    let role: { data: {} } = await getAuthClient().get("roles?name=" + name);
    return RoleModel.MapModel(role.data);
  };

  static AddPermissionToRole = async (data: RolePermissionType): Promise<AxiosResponse> => {
    return getAuthClient().post(`roles/${data.roleId}/permissions`, JSON.stringify(data.permission));
  };

  static RemovePermissionFromRole = async (roleId: string, permissionId: string): Promise<AxiosResponse> => {
    return getAuthClient().delete(`roles/${roleId}/permissions/` + permissionId);
  };

  static PrepareModel(data: RoleModel) {
    data.name = data.name?.trim();
    data.description = data.description?.trim();
    return data;
  }
}

export class GroupTypeClient {
  static GetAllGroupTypes = async (): Promise<GroupTypeModel[]> => {
    let groupTypes: { data: [] } = await getAuthClient().get("grouptype");
    return groupTypes.data.map((x) => GroupTypeModel.MapModel(x));
  };
}

export class GroupClient {

  static GetAllGroups = async (request?: BatchRequestDto): Promise<BatchDto<GroupModel>> => {
    let groups: { data: BatchDto<GroupModel> } = await getAuthClient().post("groups/batch", JSON.stringify(request));
    return groups.data;
  };

  static GetGroupbyName = async (name: string): Promise<GroupModel> => {
    let group: { data: {} } = await getAuthClient().get("groups?name=" + name);
    return GroupModel.MapModel(group.data);
  };

  static AddGroup = async (data: GroupModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().post("groups", JSON.stringify(data));
  };

  static UpdateGroup = async (data: GroupModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().put("groups", JSON.stringify(data));
  };

  static DeleteGroup = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("groups/" + id);
  };

  static AddUserToGroup = async (data: GroupUserType): Promise<AxiosResponse> => {
    return getAuthClient().post(`groups/${data.groupId}/users`, JSON.stringify(data.user));
  };

  static RemoveUserFromGroup = async (groupId: string, userId: string): Promise<AxiosResponse> => {
    return getAuthClient().delete(`groups/${groupId}/users/${userId}`);
  };

  static PrepareModel(data: GroupModel) {
    data.name = data.name?.trim();
    return data;
  }
}

export class PermissionClient {

  static GetAllPermissions = async (request?: BatchRequestDto): Promise<BatchDto<PermissionModel>> => {
    let permissions: { data: BatchDto<PermissionModel> } = await getAuthClient().post("permissions/batch", JSON.stringify(request));
    return permissions.data;
  };

  static GetPermissionsByRole = async (roleId: string, request: BatchRequestDto, getOnlyNonMembers: boolean): Promise<BatchDto<PermissionModel>> => {
    let batch: { data: BatchDto<PermissionModel> } = await getAuthClient().post("permissions/batch/role?roleId=" + roleId + "&getOnlyNonMembers=" + getOnlyNonMembers, JSON.stringify(request));
    return batch.data;
  };


  static AddPermission = async (data: PermissionModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().post("permissions", JSON.stringify(data));
  };

  static UpdatePermission = async (data: PermissionModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().put("permissions", JSON.stringify(data));
  };

  static DeletePermission = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("permissions/" + id);
  };

  static PrepareModel(data: PermissionModel) {
    data.name = data.name?.trim();
    data.description = data.description?.trim();
    return data;
  }
}

export class ApplicationApiClient {
  static GetAllApplications = async (options?: FilterOptions): Promise<ApplicationModel[]> => {
    let apps: { data: [] } = await getAuthClient().get("application?" + ClientHelper.ToQueryString(options));
    return apps.data.map((x) => ApplicationModel.MapModel(x));
  };

  static GetApplicationByName = async (applicationName: string): Promise<ApplicationModel> => {
    let apps: { data: [] } = await getAuthClient().get(`application/${applicationName}`);
    return ApplicationModel.MapModel(apps.data);
  };

  static GetApplicationClients = async (applicationName: string, options?: FilterOptions): Promise<ApplicationClientModel[]> => {
    let apps: { data: [] } = await getAuthClient().get(`application/${applicationName}/clients?` + ClientHelper.ToQueryString(options));
    return apps.data.map((x) => ApplicationClientModel.MapModel(x));
  };

  static GetClientCredentials = async (clientIds: string[]): Promise<{ [key: string]: ClientAppPasswordModel }> => {
    let creds: { data: { [key: string]: ClientAppPasswordModel } } = await getAuthClient().get("application/clients/credentials?" + ClientHelper.ToQueryStringValues("clientIds", clientIds));
    return creds.data;
  };

  static AddApplicationClient = async (data: ApplicationClientModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().post("application/clients", JSON.stringify(data));
  };

  static UpdateApplicationClient = async (data: ApplicationClientModel): Promise<AxiosResponse> => {
    data = this.PrepareModel(data);
    return getAuthClient().put("application/clients", JSON.stringify(data));
  };

  static GenerateClientSecret = async (applicationName: string, clientName: string): Promise<ClientAppPasswordModel> => {
    var result = await getAuthClient().post(`application/${applicationName}/clients/${clientName}`);
    return result.data as ClientAppPasswordModel;
  };

  static DeleteApplicationClient = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("application/clients/" + id);
  };

  static PrepareModel(data: ApplicationClientModel) {
    data.name = data.name?.trim();
    data.description = data.description?.trim();
    return data;
  }
}

export class AssignmentClient {

  static GetAllAssignments = async (userBatchRequest: BatchRequestDto, groupBatchRequest: BatchRequestDto): Promise<BatchDto<AssignmentModel>> => {
    const [UserAssignmentBatch, GroupAssignmentBatch] = await Promise.all([this.GetUserAssignmentBatch(userBatchRequest),this.GetGroupAssignmentBatch(groupBatchRequest)]);
    const result = new BatchDto<AssignmentModel>();
    result.items = UserAssignmentBatch.items.concat(GroupAssignmentBatch.items);
    result.total = UserAssignmentBatch.total + GroupAssignmentBatch.total;
    return result;
  };

  static GetUserAssignmentBatch = async (batchRequest: BatchRequestDto): Promise<BatchDto<AssignmentModel>> => {
    const assignments: { data: BatchDto<AssignmentModel> } = await getAuthClient().post("assignments/user/batch", JSON.stringify(batchRequest));
    assignments.data.items = assignments.data.items.map((x: any) => AssignmentModel.MapModel(x, 'U'));
    return assignments.data;
  };

  static GetGroupAssignmentBatch = async (batchRequest: BatchRequestDto): Promise<BatchDto<AssignmentModel>> => {
    const assignments: { data: BatchDto<AssignmentModel> } = await getAuthClient().post("assignments/group/batch", JSON.stringify(batchRequest));
    assignments.data.items = assignments.data.items.map((x: any) => AssignmentModel.MapModel(x, 'G'));
    return assignments.data;
  };

  static AddUserAssignment = async (data: UserRoleAssignmentModel): Promise<AxiosResponse> => {
    return getAuthClient().post("assignments/user", JSON.stringify(data));
  };

  static AddGroupAssignment = async (data: GroupRoleAssignmentModel): Promise<AxiosResponse> => {
    return getAuthClient().post("assignments/group", JSON.stringify(data));
  };

  static EditUserAssignment = async (data: UserRoleAssignmentModel): Promise<AxiosResponse> => {
    return getAuthClient().put("assignments/user", JSON.stringify(data));
  };

  static EditGroupAssignment = async (data: GroupRoleAssignmentModel): Promise<AxiosResponse> => {
    return getAuthClient().put("assignments/group", JSON.stringify(data));
  };

  static DeleteUserAssignment = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("assignments/user/" + id);
  };

  static DeleteGroupAssignment = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("assignments/group/" + id);
  };

  static ValidateExpression = async (data: string): Promise<AxiosResponse> => {
    return getAuthClient().post("assignments/validate", JSON.stringify(data));
  }

  static GetClientAssignments = async (applicationName: string): Promise<ClientAssignmentModel[]> => {
    let clientAssignments: { data: [] } = await getAuthClient().get(`assignments/client/${applicationName}`);
    return clientAssignments.data.map((x) => ClientAssignmentModel.MapModel(x));
  };

  static AddClientAssignment = async (data: ClientAssignmentModel): Promise<AxiosResponse> => {
    return getAuthClient().post("assignments/client", JSON.stringify(data));
  };


  static EditClientAssignment = async (data: ClientAssignmentModel): Promise<AxiosResponse> => {
    return getAuthClient().put("assignments/client", JSON.stringify(data));
  };

  static DeleteClientAssignment = async (id: string): Promise<AxiosResponse> => {
    return getAuthClient().delete("assignments/client/" + id);
  };


}

export class ImportExportClient {

  static GetSupportedEntityTypes = async (): Promise<string[]> => {
    let recordTypes: { data: {} } = await getAuthClient().get("importexport/recordTypes");
    return recordTypes.data as string[];
  };

  static ExportData = async (recordTypes: string[]): Promise<FileResponse> => {
    let options_: AxiosRequestConfig = {
      responseType: "blob",
      headers: {
        "Accept": "application/zip"
      }
    };
    return getAuthClient('').get("importexport/export?" + ClientHelper.ToQueryStringValues('recordTypes', recordTypes), options_)
      .then(response => ClientHelper.ToFileResponse(response));
  };

  static ImportData = async (file: File): Promise<FileResponse> => {
    const formData = new FormData();
    formData.append("formFile", file, 'formFile');

    let options_: AxiosRequestConfig = {
      responseType: "blob",
      headers: {
        "Accept": "application/zip",
        'Content-Type': 'multipart/form-data',

      }
    };
    return getAuthClient().post("importexport/import", formData, options_).then(response => ClientHelper.ToFileResponse(response));
  };

}

export class TwinsClient {

  static GetLocationTwins = async (): Promise<ILocationTwinModel[]> => {
    let locations: { data: [] } = await getAuthClient().get("twins/locations");
    return locations.data as ILocationTwinModel[];
  }
}

export class ConfigClient {

  static GetConfig = async (): Promise<ConfigModel> => {
    const response = await getAuthClient().get("config");
    return ConfigModel.MapModel(response.data);
  }
}

export class AuthorizationClient {

  static GetAuthorizationData = async (): Promise<AuthorizationModel> => {
    const response = await getAuthClient().get("authorization");
    return AuthorizationModel.MapModel(response.data);
  }
}


export interface FileResponse {
  data: Blob;
  status: number;
  fileName?: string;
  headers?: { [name: string]: any };
}

export class ClientHelper {

  static ToQueryString = (payload?: object): string => {
    if (payload === null || payload === undefined)
      return '';
    return Object.entries(payload).map(([key, val]) => `${key}=${val}`).join('&');
  };

  static ToQueryStringValues = (key: string, values: string[]) => {
    return values.map(val => `${key}=${val}`).join('&');
  };

  static GetQueryString(obj: any, prefix: string = ''): string {
    const params = new URLSearchParams();

    const addParam = (key: string, value: any) => {
      if (Array.isArray(value)) {
        value.forEach((v, i) => addParam(`${key}[${i}]`, v));
      } else if (value !== null && typeof value === 'object') {
        Object.keys(value).forEach((k) => addParam(`${key}.${k}`, value[k]));
      } else if (value !== undefined) {
        params.append(key, value);
      }
    };

    Object.keys(obj).forEach((key) => {
      const value = obj[key];
      const paramKey = prefix ? `${prefix}.${key}` : key;
      addParam(paramKey, value);
    });

    return params.toString();
  }

  static ToFileResponse(response: AxiosResponse): Promise<FileResponse> {
    const status = response.status;
    let _headers: any = {};
    if (response.headers && typeof response.headers === "object") {
      for (let k in response.headers) {
        if (response.headers.hasOwnProperty(k)) {
          _headers[k] = response.headers[k];
        }
      }
    }
    const contentDisposition = response.headers ? response.headers["content-disposition"] : undefined;
    let fileNameMatch = contentDisposition ? /filename\*=(?:(\\?['"])(.*?)\1|(?:[^\s]+'.*?')?([^;\n]*))/g.exec(contentDisposition) : undefined;
    let fileName = fileNameMatch && fileNameMatch.length > 1 ? fileNameMatch[3] || fileNameMatch[2] : undefined;
    if (fileName) {
      fileName = decodeURIComponent(fileName);
    } else {
      fileNameMatch = contentDisposition ? /filename="?([^"]*?)"?(;|$)/g.exec(contentDisposition) : undefined;
      fileName = fileNameMatch && fileNameMatch.length > 1 ? fileNameMatch[1] : undefined;
    }
    return Promise.resolve({ fileName: fileName, status: status, data: new Blob([response.data], { type: response.headers["content-type"] }), headers: _headers });
  }
}
