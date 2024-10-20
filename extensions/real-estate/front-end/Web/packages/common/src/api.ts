import qs from 'qs'

// Our multi-tenant instances use a cookie called `api` to store
// the user's current region, which is added to the paths of API
// calls to route requests to the right servers. This is legacy;
// it will be removed when all our customers are in single tenant.

let globalPrefix = ''

export function getApiGlobalPrefix() {
  return globalPrefix
}

export function setApiGlobalPrefix(
  prefix: string | undefined,
  isMobile = false
) {
  let nextPrefix = prefix
  if (prefix == null || !['au', 'us', 'eu'].includes(prefix)) {
    nextPrefix = 'us'
  }

  document.cookie = `api=${nextPrefix};path=/;expires=Tue, 19 Jan 2038 03:14:07 UTC`
  globalPrefix = `${isMobile ? '/mobile-web' : ''}/${nextPrefix}`
  return globalPrefix
}

function _getUrl(
  path: string,
  options: { params?: any; ignoreGlobalPrefix?: boolean },
  isMobile = false
) {
  if (path == null) {
    return null
  }

  const queryString = qs.stringify(options?.params, { arrayFormat: 'repeat' })

  // The mobile app runs on /mobile-web now.
  let appPrefix =
    isMobile && !globalPrefix.includes('/mobile-web') ? '/mobile-web' : ''
  let nextUrl =
    !options?.ignoreGlobalPrefix && !path?.startsWith('http')
      ? `${appPrefix}${globalPrefix}${path}`
      : `${appPrefix}${path}`

  if (queryString !== '') {
    const separator = nextUrl.includes('?') ? '&' : '?'
    nextUrl = `${nextUrl}${separator}${queryString}`
  }

  return nextUrl
}

export default function getUrl(
  path: string,
  options?: { params?: any; ignoreGlobalPrefix?: boolean }
) {
  return _getUrl(path, options ?? {})
}

export function getMobileUrl(
  path: string,
  options?: { params?: any; ignoreGlobalPrefix?: boolean }
) {
  return _getUrl(path, options ?? {}, true)
}
