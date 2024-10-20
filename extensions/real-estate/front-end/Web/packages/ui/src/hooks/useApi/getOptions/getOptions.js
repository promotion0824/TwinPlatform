import { caseInsensitiveEquals } from '@willow/ui'
import getBody from './getBody'
import getContentType from './getContentType'

export default function getOptions(options) {
  const contentType = getContentType(options)
  const body = getBody(contentType, options)

  const nextHeaders = {
    ...options?.headers,
    ...(contentType != null ? { 'Content-Type': contentType } : {}),
    language: window.localStorage.getItem('i18nextLng') || 'en',
  }

  if (contentType === 'multipart/form-data') {
    const contentTypeKey = Object.entries(nextHeaders).find((entry) =>
      caseInsensitiveEquals(entry[0], 'content-type')
    )?.[0]

    delete nextHeaders[contentTypeKey]
  }

  return {
    method: 'get',
    responseType: 'json',
    ...options,
    body,
    headers: nextHeaders,
  }
}
