import qs from 'qs'

export default {
  get(key) {
    const parsed = qs.parse(window.location.search.slice(1), {
      arrayLimit: 1000,
    })

    return parsed?.[key] ?? undefined
  },

  parse() {
    return qs.parse(window.location.search.slice(1))
  },

  createUrl(url, params, options) {
    const queryString = qs.stringify(params, options)

    return queryString.length > 0 ? `${url}?${queryString}` : url
  },
}
