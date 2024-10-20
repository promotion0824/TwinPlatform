/// <reference types="cypress" />
const downloadsFolder = Cypress.config('downloadsFolder')
describe('Testing Jobs', () => {
  before('Login and get all tokens ', () => {
      cy.LoginLS();
      cy.saveLocalStorage('AfterLogin');
  });
    beforeEach('Inject tokens and navigate to Jobs Page', () => {
        cy.restoreLocalStorage('AfterLogin');
        cy.navigateToJobsPage();
    });

    it('Jobs - Refresh button validation', function () {
        cy.get('button[data-cy=refresh-button]').should('include.text', 'Refresh'); //Verifies Refresh button
    });
    it('Jobs - Status checkbox validation - nothing selected', function () {
        cy.SetJobFilter(false, false, false, false, false);
        cy.contains('Done').click();
    });
    it('Jobs - Status checkbox validation - Done selected', function () {
        cy.SetJobFilter(true, false, false, false, false);
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'No rows'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Error');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Processing'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Queued');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Canceled'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('include.text', 'Done');
    });
    it('Jobs - Status checkbox validation - Error selected', function () {
        cy.SetJobFilter(false, true, false, false, false);
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'No rows'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Done');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Processing'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Queued');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Canceled'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('include.text', 'Error');
    });
    it('Jobs - Status checkbox validation - Processing selected', function () {
        cy.SetJobFilter(false, false, true, false, false);
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'No rows'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Done');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Error');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Queued');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Canceled'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('include.text', 'Processing');
    });
    it('Jobs - Status checkbox validation - Queued selected', function () {
        cy.SetJobFilter(false, false, false, true, false);
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'No rows'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Done');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Error');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Processing'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Canceled'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('include.text', 'Queued');
    });
    it('Jobs - Status checkbox validation - Canceled selected', function () {
        cy.SetJobFilter(false, false, false, false, true);
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'No rows'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Done');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Error');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'not.include.text',
            'Processing'
        );
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('not.include.text', 'Queued');
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should('include.text', 'Canceled');
    });
    it('Jobs - Validate Jobs details Page', function () {
        cy.SetJobFilter(false, false, false, true, false);
        cy.get('.MuiDataGrid-cellContent').contains('willowinc.com').first().click();
        cy.get('[data-cy="all-jobs-button"]').should('be.visible');
        cy.get('[data-cy="refresh-job-details-button"]').should('be.visible');
        cy.get('[data-cy="cancel-job-button"]').should('be.visible');

        cy.get('.container').first().should('include.text', 'willowinc.com');
        cy.get('.container').first().should('include.text', 'Queued');

        cy.get('[data-cy="refresh-job-details-button"]').click();
        cy.get('.container').first().should('include.text', 'willowinc.com');
        cy.get('.container').first().should('include.text', 'Queued');

        cy.get('[data-cy="all-jobs-button"]').should('be.visible').click();
        cy.get('.MuiDataGrid-cellContent').contains('willowinc.com').should("be.visible");
        
    });

    it('Jobs - Validate Export Button', function () {
        cy.get('.MuiDataGrid-virtualScroller.css-1w5m2wr-MuiDataGrid-virtualScroller').should(
            'include.text',
            'Processing'
        );
        cy.get('#mui-5').click();
        cy.get('#mui-6').children().first().click();
        cy.readFile(downloadsFolder +'\\Willow.TwinLifecycleManagement.Web.csv').should('exist');
    });
});
