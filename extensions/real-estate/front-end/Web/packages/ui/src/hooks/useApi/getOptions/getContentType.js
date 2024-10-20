import { caseInsensitiveEquals } from '@willow/ui'

export default function getContentType(options) {
  const contentType =
    options?.headers != null
      ? Object.entries(options.headers).find((entry) =>
          caseInsensitiveEquals(entry[0], 'content-type')
        )?.[1]
      : undefined

  if (options?.body == null && contentType == null) {
    return undefined
  }

  return contentType ?? 'application/json; charset=utf-8'
}
