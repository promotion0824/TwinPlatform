import qs from 'qs'
import { caseInsensitiveEquals } from '@willow/ui'
import getFormData from './getFormData'

export default function getBody(contentType, options) {
  if (options?.body == null || options?.handleBody) {
    return options?.body
  }

  if (caseInsensitiveEquals(contentType, 'application/x-www-form-urlencoded')) {
    return qs.stringify(options.body, { arrayFormat: 'repeat' })
  }

  if (caseInsensitiveEquals(contentType, 'multipart/form-data')) {
    return getFormData(options.body)
  }

  if (caseInsensitiveEquals(contentType?.slice(0, 16), 'application/json')) {
    return JSON.stringify(options.body)
  }

  return options.body
}
