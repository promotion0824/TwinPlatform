import { getUrl } from '@willow/ui'
import axios from 'axios'
import {
  getAutoDeskAccessToken,
  getAutoDeskFileManifest,
  getAutoDeskModuleFile,
} from '../AutoDesk/AutoDeskService'

axios.defaults.baseURL = window.location.origin
export const FLOORS_API_PREFIX = 'sites'

export async function post3dModule(siteId, formData, config) {
  const POST_MODEL_UPLOAD_URL = getUrl(
    `/api/${FLOORS_API_PREFIX}/${siteId}/module`
  )
  return axios
    .post(POST_MODEL_UPLOAD_URL, formData, {
      ...config,
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    .then((res) => res.data)
}

export async function get3dModule(siteId) {
  const GET_MODELS_UPLOAD_URL = getUrl(
    `/api/${FLOORS_API_PREFIX}/${siteId}/module`
  )
  return axios.get(GET_MODELS_UPLOAD_URL).then(({ data }) => data)
}

export async function delete3dModule(siteId) {
  const DELETE_MODEL_UPLOAD_URL = getUrl(
    `/api/${FLOORS_API_PREFIX}/${siteId}/module`
  )
  return axios.delete(DELETE_MODEL_UPLOAD_URL).then(({ data }) => data.status)
}

export async function get3dModuleFile(urn, fileName) {
  const { access_token: accessToken, token_type: tokenType } =
    await getAutoDeskAccessToken()
  const authorization = `${tokenType} ${accessToken}`

  const { fileInfo, progress } = await getAutoDeskFileManifest(
    urn,
    authorization
  )

  if (progress !== 'complete') {
    throw new Error(`Autodesk is still processing. ${progress}`)
  }
  const { urn: derivativeUrn } = fileInfo
  return getAutoDeskModuleFile(urn, derivativeUrn, authorization).then(
    (res) => new File([res], fileName)
  )
}
