import qs from 'qs'
import { api } from '@willow/ui'
import {
  getApiGlobalPrefix,
  setApiGlobalPrefix as setApiGlobalPrefixBase,
} from '@willow/common/api'

export { getApiGlobalPrefix }

export function setApiGlobalPrefix(prefix) {
  const globalPrefix = setApiGlobalPrefixBase(prefix)
  api.defaults.baseURL = `${globalPrefix}/api`
}

export default function getUrl(url, options) {
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
