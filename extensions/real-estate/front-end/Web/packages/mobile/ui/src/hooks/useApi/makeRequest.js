import axios from 'axios'
import cookie from 'utils/cookie'

function sleep(ms) {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

export default async function makeRequest(method, url, data, config) {
  // eslint-disable-line
  if (
    config.mock &&
    (!config.mockToggle || cookie.parse()?.[config.mockToggle] === true)
  ) {
    await sleep(config.mockTimeout)

    return config.mock
  }

  let response
  if (method === 'get') response = await axios.get(url, config)
  if (method === 'post') response = await axios.post(url, data, config)
  if (method === 'put') response = await axios.put(url, data, config)
  if (method === 'delete') response = await axios.delete(url, data, config)

  return response
}
