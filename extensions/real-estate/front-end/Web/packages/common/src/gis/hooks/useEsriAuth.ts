import { useQuery } from 'react-query'
import { api } from '@willow/ui'
import IdentityManager from '@arcgis/core/identity/IdentityManager'
import Config from '@arcgis/core/config'

export const tokenMissingError = 'token is missing'

declare module 'axios' {
  export interface AxiosRequestConfig {
    referer?: string
  }
}

const useEsriAuth = (siteId: string) =>
  useQuery<unknown, Error, boolean>(['esri-auth', siteId], async () => {
    try {
      let {
        data: { gisBaseUrl, token, authRequiredPaths, gisPortalPath, message },
      } = await api.get(`/sites/${siteId}/arcGisToken`, {})

      // The `arcGisToken` request can fail to retrieve the expected data but
      // still return a successful response code. One way to detect this is by
      // the presence of a `message` attribute in the output, another is by the
      // absence of a token.
      // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/76281
      if (message) {
        throw new Error(message)
      }
      if (!token) {
        throw new Error(tokenMissingError)
      }

      if (process.env.ARCGIS_BASE_URL != null) {
        gisBaseUrl = process.env.ARCGIS_BASE_URL
      }

      // setting the url of the portal instance.
      // This the recommended way to set url for a portal as per following:
      // https://developers.arcgis.com/javascript/latest/api-reference/esri-portal-Portal.html#url
      // https://developers.arcgis.com/javascript/latest/api-reference/esri-config.html#portalUrl
      Config.portalUrl = `${gisBaseUrl}${gisPortalPath}`
      authRequiredPaths.forEach((authPath: string) => {
        // Note - you will get a cryptic error from this function if the
        // `server` URL is wrong or malformed.
        IdentityManager.registerToken({
          server: `${gisBaseUrl}${authPath}`,
          token,
          ssl: true,
        })
      })

      return true
    } catch (e) {
      console.error(e)
      throw e
    }
  })

export default useEsriAuth
