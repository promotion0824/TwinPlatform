import axios from 'axios'
import { getUrl } from '@willow/ui'

import {
  ExistingModelOfInterest,
  PartialModelOfInterest,
} from '../../views/Admin/ModelsOfInterest/types'

export function postModelOfInterest(
  customerId: string,
  formData: PartialModelOfInterest,
  etag: string
) {
  const postModelOfInterestUrl = getUrl(
    `/api/customers/${customerId}/modelsOfInterest`
  )

  return axios
    .post(postModelOfInterestUrl, formData, {
      headers: {
        'If-Match': etag,
        'Content-Type': 'application/json',
      },
    })
    .then(({ data }) => data)
}

export function putModelOfInterest(
  customerId: string,
  modelOfInterest: ExistingModelOfInterest,
  etag: string
) {
  const putModelOfInterestUrl = getUrl(
    `/api/customers/${customerId}/modelsOfInterest/${modelOfInterest.id}`
  )

  return axios.put(putModelOfInterestUrl, modelOfInterest, {
    headers: {
      'If-Match': etag,
      'Content-Type': 'application/json',
    },
  })
}

export function deleteModelOfInterest(
  customerId: string,
  id: string,
  etag: string
) {
  const deleteModelOfInterestUrl = getUrl(
    `/api/customers/${customerId}/modelsOfInterest/${id}`
  )

  return axios.delete(deleteModelOfInterestUrl, {
    headers: {
      'If-Match': etag,
      'Content-Type': 'application/json',
    },
  })
}
