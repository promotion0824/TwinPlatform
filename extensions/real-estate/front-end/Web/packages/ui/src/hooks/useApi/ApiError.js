export default class ApiError extends Error {
  constructor(err) {
    const status = err?.status
    const description =
      status != null
        ? `Request failed with status code ${status}`
        : 'Request failed'

    super(description)

    this.name = 'ApiError'
    this.status = status
    this.url = err?.url
    this.data = err?.data
    this.response = err?.response
  }
}
