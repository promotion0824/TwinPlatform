class ProviderRequiredError extends Error {
  constructor(providerName) {
    super(`use${providerName} requires a ${providerName}`)
    this.name = 'ProviderRequiredError'
  }
}

export default ProviderRequiredError
