import axios, { AxiosRequestConfig } from 'axios'
import { getUrl } from '@willow/ui'

/**
 * As we do not store and manage uploaded files,Direct access to uploded file requires to use APIs from AutoDesk
 * AccessToken API is only exceptional that we use from BE. (AutoDesk API Token is delivered via BE due to cors issue)
 */

export async function getAutoDeskAccessToken(): Promise<TokenResponse> {
  const GET_AUTO_DESK_OAUTH_TOKEN_URL = getUrl('/api/forge/oauth/token')
  return axios.get(GET_AUTO_DESK_OAUTH_TOKEN_URL).then(({ data }) => data)
}

export type TokenResponse = {
  access_token: string
  token_type: string
}

const AUTODESK_API_BASE_URL =
  'https://developer.api.autodesk.com/modelderivative/v2/designdata'
export async function getAutoDeskFileManifest(urn, authorization) {
  const config = {
    headers: { Authorization: authorization },
  }
  return axios
    .get(`${AUTODESK_API_BASE_URL}/${urn}/manifest`, config)
    .then(({ data }) => {
      const { progress } = data
      const fileInfo = data?.derivatives[0]?.children[0]?.children.find(
        ({ role }) => role === 'graphics'
      )
      return { progress, fileInfo }
    })
}

export async function getAutoDeskModuleFile(urn, derivativeUrn, authorization) {
  const config: AxiosRequestConfig = {
    headers: { Authorization: authorization },
    responseType: 'stream',
  }

  const get3dModuleFileUrl = `${AUTODESK_API_BASE_URL}/${urn}/manifest/${derivativeUrn}`
  return axios.get(get3dModuleFileUrl, config).then(({ data }) => data)
}
