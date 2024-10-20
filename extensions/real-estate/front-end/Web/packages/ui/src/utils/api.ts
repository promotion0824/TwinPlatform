import axios from 'axios'
import qs from 'qs'

import { getApiGlobalPrefix } from './getUrl'

// This simplifies and replaces the 'useApi' and other api functionality.
export default axios.create({
  baseURL: `${getApiGlobalPrefix()}/api`,
  // .NET wants its arrays in a query string like a=1&a=2, not a[]=1&a[]=2
  paramsSerializer: (params) => qs.stringify(params, { arrayFormat: 'repeat' }),
})
