const date = new Date()
const currentDate = date.getDate()

describe('Customer Admin ticket creation', () => {
  const user = {
    email: Cypress.env('email'),
    password: Cypress.env('password'),
  }

  beforeEach(() => {
    cy.loginAU(user)
  })

  it('should be able to create a standard ticket.', () => {
    cy.visit('/')
    cy.selectSite('60 Martin Place')
    cy.get('[data-testid="insight-result"]').should('have.length.above', 0)
    cy.get('[data-cy=header-hamburger-menu]').click()
    cy.get('[data-testid=tickets-menu-button]').click()
    cy.contains('New ticket').click()
    cy.get('[data-cy=asset-ticket-floorCode]').click()
    cy.contains('L36').click()
    cy.get('[data-cy=ticketDetails-ticket-summary]').type(
      `Cypress UI ticket: ${currentDate}`
    )
    cy.get('[data-cy=ticketDetails-ticket-description]').type(
      'Cypress ticket description'
    )
    cy.get('[data-cy=requestor-name]').type('Cypress user')
    cy.get('[data-cy=requestorDetails-phone]').type('0455555555')
    cy.get('[data-cy=requestorDetails-email]').type('qa+au_ca@willowinc.com')
    cy.get('[data-testid=modalButton-submit]').click()
    cy.contains('Ticket Id:')
    cy.contains('60MP-T-')
  })

  it('should be able to create a ticket from insight', () => {
    cy.visit('/')
    cy.intercept({
      method: 'POST',
      url: '**/tickets',
    }).as('getPost')
    cy.selectSite('60 Martin Place')
    cy.get('[data-cy=header-hamburger-menu]').click()
    cy.get('[data-testid=insights-menu-button]').click()
    cy.get('[data-segment="Insight Selected"]')
      .eq(0)
      .get('[data-testid=insight-sequence-number-cell]')
      .eq(0)
      .click()
    cy.get('[data-segment="Insight Create Ticket Clicked"]').click()
    cy.get('[data-testid=modalButton-submit]').click()
    // Assert that the ticket is successfully created
    cy.wait('@getPost').then((xhr) => {
      expect(xhr.response.statusCode).equal(200)
    })
    // Assert that Ticket Id label appears after record is saved
    cy.contains('Ticket Id:')
  })
})
