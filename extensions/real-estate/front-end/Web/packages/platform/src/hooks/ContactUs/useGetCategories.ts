import { useQuery } from 'react-query'
import { getUrl } from '@willow/ui'
import axios from 'axios'

/**
 * Retrieve categories for Contact Us Form
 */
export default function useGetCategories() {
  return useQuery<
    {
      value: string
      label: string
    }[]
  >(['contactUsCategories'], async () => {
    const getCategoryUrl = getUrl(`/api/contactus/categories`)

    return axios
      .get(getCategoryUrl)
      .then(({ data }) => data.map(({ value }) => ({ value, label: value })))
  })
}
