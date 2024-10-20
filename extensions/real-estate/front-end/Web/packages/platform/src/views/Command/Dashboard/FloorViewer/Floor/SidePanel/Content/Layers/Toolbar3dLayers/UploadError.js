export default class UploadError extends Error {
  constructor(response) {
    super('An error has occurred')

    this.response = response
  }
}
