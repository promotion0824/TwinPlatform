// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add('drag', { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add('dismiss', { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite('visit', (originalFn, url, options) => { ... })

/**
 * Authenticate user login command for AU instance.
 * This will authenticate the user only if the user has not been authenticated once.
 * Subsequent calls where the session has been cached, Cypress will just load cached login session.
 * If the session is not to be cached, then call to this command is made by passing cacheSession: false.
 */
Cypress.Commands.add('loginAU', (user, { cacheSession = true } = {}) => {
  const login = () => {
    cy.visit('/')
    // Set cookie so user login is done in AU region instead of default US region
    cy.setCookie('api', 'au')
    cy.get('#signInName').type(user.email + '{enter}')
    cy.get('#password').type(user.password + '{enter}')
    cy.get('[data-cy=header-hamburger-menu]', { timeout: 10000 })
  }
  if (cacheSession) {
    cy.session(user, login)
  } else {
    login()
  }
})

/**
 * Common command to select specific site from site list dropdown
 * by passing the site name to this command.
 */
Cypress.Commands.add('selectSite', (siteName) => {
  cy.get('[data-cy=siteSelect-dropdown]', { timeout: 10000 }).click()
  cy.get('[data-cy=site-option-dropdown]', { timeout: 10000 })
    .contains(siteName)
    .click()
})
