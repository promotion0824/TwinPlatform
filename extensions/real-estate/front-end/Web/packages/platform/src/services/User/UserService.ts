import { getUrl } from '@willow/ui'
import axios from 'axios'

type Portfolio = {
  portfolioId: string
  portfolioName: string
  role: string
  sites: Array<{
    siteId: string
    siteName: string
    role: string
  }>
}

export type UserForm = {
  firstName: string
  lastName: string
  email: string
  contactNumber: string
  company: string
  useB2C: boolean
  isCustomerAdmin: boolean
  portfolios: Array<Portfolio>
}

class ValidationError extends Error {
  items: any[]

  constructor(message, items = []) {
    super(message)
    this.items = items
  }
}
export default async function postUser(
  id: string,
  formData: UserForm,
  headers: Record<string, any>
) {
  const postUserUrl = getUrl(`/api/management/customers/${id}/users`)
  return axios
    .post(postUserUrl, formData, { headers })
    .then((res) => res.data)
    .catch((e) => {
      if (e.response?.data?.items) {
        /**
         * items is the error format from API error response.
         * it is mainly used for the form component that automatically display validation error messages below input component
         * error response format
         * {
         *   items: [{ name: "email", message: "email is required"}]
         * }
         */
        throw new ValidationError('Form is invalid', e.response.data?.items)
      }
      throw e
    })
}
