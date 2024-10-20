export default class AbortError extends DOMException {
  constructor() {
    const message = 'The user aborted a request'

    super(message, 'AbortError')
  }
}
