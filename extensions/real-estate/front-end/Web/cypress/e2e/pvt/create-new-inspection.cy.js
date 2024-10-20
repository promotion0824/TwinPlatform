const date = new Date()
const currentDate = date.getDate()

describe('Customer Admin inspection creation', () => {
  const user = {
    email: Cypress.env('email'),
    password: Cypress.env('password'),
  }

  beforeEach(() => {
    cy.loginAU(user)
  })

  it('should be able to create an inspection', () => {
    cy.visit('/')
    cy.intercept({
      method: 'POST',
      url: '**/inspections',
    }).as('getPost')
    // Test run against Willow Demo site
    cy.selectSite('Willow Demo')
    cy.get('[data-cy=header-hamburger-menu]').click()
    cy.get('[data-testid=inspections-menu-button]').click()
    cy.get('[data-cy=inspections-zones-button]').click()
    cy.get('[data-cy=zones-list-item]').eq(0).click()
    cy.get('[data-cy=add-inspection-button]', { timeout: 20000 }).click()
    cy.get('[data-cy=inspection-floor-select]', { timeout: 10000 }).click()
    // A floor that has assets
    cy.contains('L8').click()
    cy.get('[data-cy=inspection-asset-input]').type(' ')
    cy.get('[data-cy=inspection-asset-selection]', { timeout: 10000 })
      .eq(0)
      .click()
    cy.get('[data-cy=inspection-group-select]').click()
    cy.get('[data-cy=inspection-group-option]', { timeout: 10000 })
      .eq(0)
      .click()
    cy.get('[data-cy=inspection-start-date]').click()
    cy.contains(`${currentDate}`).click()
    cy.get('[data-cy=inspection-start-date]').click()
    cy.get('[data-cy=inspection-add-check]').click()
    cy.get('[data-cy=inspection-check-title]').type(
      'Automation Inspection Check'
    )
    cy.get('[data-cy=inspection-checkType-select]').click()
    cy.get('[data-cy=inspection-checkType-total]').click()
    cy.get('[data-cy=inspection-check-totalValue]').type('100')
    cy.get('[data-cy=inspection-check-decimalPlaces]').type('2')
    cy.get('[data-cy=inspection-check-submit]').click()
    cy.get('[data-testid=modalButton-submit]').click()
    // Assert that the record is successfully created
    cy.wait('@getPost').then((xhr) => {
      expect(xhr.response.statusCode).equal(200)
    })
  })
})
