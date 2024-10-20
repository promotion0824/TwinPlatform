import { useMutation } from 'react-query'
import { getUrl } from '@willow/ui'
import axios from 'axios'
import { CustomerFormDetails } from '../../views/Layout/Layout/Header/ContactUs/types'

/**
 * Creating ticket based on Contact Us form details
 */
export default function useCreateTicket() {
  return useMutation((formData: CustomerFormDetails) => {
    const createTicketUrl = getUrl(`/api/contactus`)

    // Use FormData instead of a regular object to prevent boundary not found errors
    // It will also remove empty and null values from the formData
    // and convert individual array items to FormData instead of string format
    // reference: https://stackoverflow.com/questions/49579640/how-to-send-data-correct-axios-error-multipart-boundary-not-found
    const dataToSubmit = new FormData()
    Object.entries(formData).forEach(([key, value]) => {
      if (
        key === 'attachmentFiles' &&
        Array.isArray(formData.attachmentFiles)
      ) {
        // Append attachments separately
        formData.attachmentFiles.forEach((file) => {
          dataToSubmit.append('attachmentFiles', file)
        })
      } else if (Array.isArray(value)) {
        value.forEach((data) => {
          dataToSubmit.append(
            key,
            typeof data !== 'string' ? JSON.stringify(data) : data
          )
        })
      } else if (value?.length > 0) {
        dataToSubmit.append(
          key,
          typeof value !== 'string' ? JSON.stringify(value) : value
        )
      }
    })

    return axios
      .post(createTicketUrl, dataToSubmit, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((res) => res.data)
  })
}
