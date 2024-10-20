describe('Login page', () => {
  it('cancel button should reload the page', () => {
    cy.visit('/')
    cy.get('#signInName').type(Cypress.env('email'))
    cy.get('#cancel')
      .click()
      .then(() => {
        cy.get('#signInName').should(
          'have.attr',
          'aria-label',
          'Enter your Email address'
        )
      })
  })

  it('forgot password should load verification page', () => {
    cy.visit('/')
    cy.get('#signInName').type(Cypress.env('email') + '{enter}')
    cy.get('#forgotPassword')
      .click()
      .then(() => {
        cy.get('#email_intro')
      })
  })
})

describe('Customer Admin user login', () => {
  const user = {
    email: Cypress.env('email'),
    password: Cypress.env('password'),
  }

  it('should authenticate and redirect user to operational page.', () => {
    cy.loginAU(user, { cacheSession: false })
    cy.visit('/')
    cy.location('pathname').should('to.have.string', '/portfolio/')
  })
})
