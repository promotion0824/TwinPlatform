import qs from 'qs'
import cookie from './cookie'
import { ApiError } from '../hooks'

function getApiPrefix() {
  // If we are on platform, the prefix is stored in
  // `packages/ui/src/hooks/useApi/getUrl.js`, if we are on mobile, the prefix
  // is stored in `packages/mobile/ui/src/hooks/useApi/useApi.js`. However in
  // both cases it is retrieved from this cookie, so we use that as a reliable
  // way to get it.
  return cookie.get('api')
}

async function fetchAssets(
  { siteId, ...params },
  { signal }: { signal?: AbortSignal } = {}
) {
  // `prefix` will be a region like "us" or "au". We assume the API is hosted
  // on the same domain as the frontend, so we prepend a slash to get an absolute
  // path with respect to the current domain.
  const url = `/${getApiPrefix()}/api/sites/${siteId}/assets?${qs.stringify(
    params
  )}`
  const response = await fetch(url, { signal })
  if (!response.ok) {
    throw new ApiError({
      url,
      status: response.status,
      data: response,
      response,
    })
  }
  return response.json()
}

export default fetchAssets
