// ***********************************************************
// This example support/index.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import './commands'

// eslint-disable-next-line consistent-return
Cypress.on('uncaught:exception', (err) => {
  if (
    err.message.includes(
      'Invalid response Content-Type: text/html, from URL: https://willowdevb2c.b2clogin.com/willowdevb2c.onmicrosoft.com//v2.0/.well-known/openid-configuration'
    )
  ) {
    return false
  }
})
