import axios from "axios";
import FormData from "form-data";
import {endpoint} from "../config"
import {CreateModule} from "../types/CreateModule";
import {BatchDeployment, Deployment} from "../types/Deployment";
import {DeploymentByModuleType} from "../types/DeploymentByModuleType";
import {UpdateConnector} from "../types/UpdateConnector";

const domain = endpoint;

export async function callPostModules(accessToken: string, module: CreateModule) {
  return axios({
    url: `${domain}/api/v1/Modules`,
    method: 'POST',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'application/json'
    },
    data: JSON.stringify(module, replacer)
  });
}

export async function callPutModules(accessToken: string, updateConnector: UpdateConnector) {
  return axios({
    url: `${domain}/api/v1/Modules/deployment-configs`,
    method: 'PUT',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'application/json'
    },
    data: JSON.stringify(updateConnector, replacer)
  });
}

export function callGetModulesSearch(accessToken: string, pageSize: number, page: number, moduleName?: string, isArchived?: boolean, moduleType?: string, deploymentId?: string, deviceName?: string | null) {
  let url = new URL(`${domain}/api/v1/Modules/search`);

  return axios({
    url: url.toString(),
    method: 'GET',
    headers: {
      Authorization: 'Bearer ' + accessToken
    },
    params:{
      isArchived: isArchived?.toString(),
      pageSize: pageSize.toString(),
      page: page.toString(),
      name: moduleName,
      moduleType: moduleType,
      deploymentIds: deploymentId,
      deviceName: deviceName || undefined,
    }
  });
}

export function callGetModuleTypesSearch(accessToken: string, pageSize: number, page: number, moduleType?: string) {
  let url = new URL(`${domain}/api/v1/ModuleTypes/search`);
  let params = url.searchParams;
  pageSize && params.append('pageSize', pageSize.toString());
  page && params.append('page', page.toString());
  moduleType && params.append('moduleType', moduleType);

  return axios({
    url: url.toString(),
    method: 'GET',
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  });
}

export async function callGetModuleTypes(accessToken: string, moduleType: string, version: string) {
  let url = new URL(`${domain}/api/v1/ModuleTypes`);
  let params = url.searchParams;
  moduleType && params.append('moduleType', moduleType);
  version && params.append('version', version);

  return axios({
    url: url.toString(),
    method: 'GET',
    responseType: 'blob',
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  });
}

export async function callPostModuleTypes(accessToken: string, formData: FormData) {
  return axios({
    url: `${domain}/api/v1/ModuleTypes`,
    method: 'POST',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'multipart/form-data'
    },
    data: formData
  });
}

export async function callGetModuleTypeVersions(accessToken: string, moduleType: string) {
  return axios({
    url: `${domain}/api/v1/ModuleTypes/versions?moduleType=${moduleType}`,
    method: 'GET',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'application/json',
    }
  });
}

export function callGetModule(accessToken: string, moduleId: string) {
  let url = new URL(`${domain}/api/v1/Modules/${moduleId}`);

  return axios({
    url: url.toString(),
    method: 'GET',
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  });
}

export async function callGetDeploymentsSearch(accessToken: string, pageSize: number, page: number, moduleId: string | null, deviceName: string | null) {
  let url = new URL(`${domain}/api/v1/Deployments/search`);
  let params = url.searchParams;
  pageSize && params.append('pageSize', pageSize.toString());
  page && params.append('page', page.toString());
  moduleId && params.append('moduleId', moduleId);
  deviceName && params.append('deviceName', deviceName);

  return axios({
    url: url.toString(),
    method: 'GET',
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  });
}

export async function callGetDeploymentManifests(accessToken: string, deploymentId: string) {
  return axios({
    url: `${domain}/api/v1/Deployments/manifests?deploymentIds=${deploymentId}`,
    method: 'GET',
    responseType: 'blob',
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  });
}

function replacer(key: any, value: any) {
  // Filtering out properties
  if (value === "") {
    return undefined;
  }

  return value;
}

export async function callPostDeployment(accessToken: string, deployment: Deployment) {
  return axios({
    url: `${domain}/api/v1/Deployments`,
    method: 'POST',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'application/json'
    },
    data: JSON.stringify(deployment, replacer)
  });
}

export async function callPostDeploymentBatch(accessToken: string, batchDeployment: BatchDeployment) {
  return axios({
    url: `${domain}/api/v1/Deployments/batch`,
    method: 'POST',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'application/json'
    },
    data: JSON.stringify(batchDeployment, replacer)
  });
}

export async function callPostDeploymentByModuleType(accessToken: string, deploymentByModuleType: DeploymentByModuleType) {
  return axios({
    url: `${domain}/api/v1/Deployments/byModuleType`,
    method: 'POST',
    headers: {
      Authorization: 'Bearer ' + accessToken,
      'Content-Type': 'application/json'
    },
    data: JSON.stringify(deploymentByModuleType, replacer)
  });
}
