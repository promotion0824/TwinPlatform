// Test data
const siteId = 'INV-60MP'
const siteName = '60 Martin Place'

describe('All features smoke test', () => {
  const user = {
    email: Cypress.env('email'),
    password: Cypress.env('password'),
  }

  beforeEach(() => {
    cy.loginAU(user)
  })

  it('Dashboard - should be able to navigate Operational tab under the feature', () => {
    cy.visit('/')
    cy.selectSite(`${siteName}`)
    //  Operational tab - Default tab
    cy.get('[data-testid="dashboard-subMenu"]').contains('Operational')
    cy.get('[data-testid=site-view-button]').should('be.visible')
    cy.contains('Insights').should('be.visible')
    cy.get('[data-testid="insight-result"]').should('have.length.above', 0)
    cy.contains('Tickets').should('be.visible')
    cy.get('[data-testid="ticket-result"]').should('have.length.above', 0)
  })

  it('Search & Explore - should be able to navigate pages / tabs under the feature', () => {
    cy.visit('/')
    cy.get('[data-cy=header-hamburger-menu]').click()
    cy.contains('Search & Explore').click()
    cy.get('[data-testid=Buildings]').click()
    cy.get('[data-testid=result-list]').should('have.length.above', 1)
    cy.get('[value=table]').click({ force: true })
    cy.get('[data-testid=display-table-list]').should('have.length.above', 1)
    cy.get(`[data-testid="search-item-${siteName}"]`).click()
    cy.get('[value=list]').click({ force: true })
    cy.get('[data-testid=result-list]').click()
    cy.get('[data-testid=tab-summary-information]').contains(`${siteId}`)
    cy.get('[data-testid=tab-relatedTwins]').click()
    cy.get('[data-testid=tab-relatedTwins-list]', {
      timeout: 10000,
    })
      .find('ul')
      .find('li')
      .should('have.length.above', 0)
    cy.get('[data-testid=tab-assetHistory]').click()
    cy.get('[data-testid=assetHistory-table]')
      .find('tr')
      .should('have.length.above', 0)
    cy.get('[data-testid=tab-sensors]').click()
    cy.get('[data-testid=twin-sensors-result]').should('have.length.above', 0)
    cy.get('[data-testid=tab-timeSeries-graph]').should('have.length.above', 0)
    cy.get('[data-testid=twin-relationshipsMap-tab]').click()
    cy.get(`[data-testid="TwinChipNode-${siteId}"]`)
    cy.get('[data-testid=expand-out-button]').contains('+')
    cy.get('[data-testid=expand-in-button]').contains('+')
    cy.get('[data-testid=twin-threeDModel-tab]').click()

    cy.intercept({
      method: 'GET',
      url: 'https://developer.api.autodesk.com/derivativeservice/v2/manifest/**',
    }).as('get3DModel')
    cy.wait('@get3DModel').then((xhr) => {
      expect(xhr.response.statusCode).equal(200)
    })
  })
})
