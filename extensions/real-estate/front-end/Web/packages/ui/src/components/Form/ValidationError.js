import _ from 'lodash'

export default class ValidationError extends Error {
  constructor(message, name) {
    let errors = []
    if (message) errors = [{ message, name }]
    if (_.isObject(message)) errors = [message]
    if (_.isArray(message)) errors = message

    const errorMessage = errors[0]?.message || 'An error has occurred'

    super(errorMessage)

    this.message = errorMessage
    this.errors = errors
    this.name = 'ValidationError'
    this.status = 422
  }
}
