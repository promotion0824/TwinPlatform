import axios from 'axios'
import qs from 'qs'
import {
  getApiGlobalPrefix,
  setApiGlobalPrefix as setApiGlobalPrefixBase,
} from '@willow/common/api'

// This simplifies and replaces the 'useApi' and other api functionality.
const api = axios.create({
  baseURL: `${getApiGlobalPrefix()}/api`,
  // .NET wants its arrays in a query string like a=1&a=2, not a[]=1&a[]=2
  paramsSerializer: (params) => qs.stringify(params, { arrayFormat: 'repeat' }),
})

export function setApiGlobalPrefix(prefix) {
  const globalPrefix = setApiGlobalPrefixBase(prefix, true)
  api.defaults.baseURL = `${globalPrefix}/api`
}

export function getUrl(url, options) {
  if (url == null) {
    return null
  }

  const globalPrefix = getApiGlobalPrefix()
  const queryString = qs.stringify(options?.params, { arrayFormat: 'repeat' })

  let nextUrl =
    !options?.ignoreGlobalPrefix && !url?.startsWith('http')
      ? `${globalPrefix}${url}`
      : url

  if (queryString !== '') {
    const separator = nextUrl.includes('?') ? '&' : '?'
    nextUrl = `${nextUrl}${separator}${queryString}`
  }

  return nextUrl
}

export default api
export { getApiGlobalPrefix }
