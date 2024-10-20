/// <reference types="cypress" />
describe('Testing Models Importing', () => {
  before('Inject Token and valdate if user is logged in', () => {
    cy.Login();
  });
    
  it('Tests Import Models - Real Estate', function () {
    cy.visit('/'); //visits Homepage
    cy.navigateToImportModelsPage();
    cy.get('#folder-path-select').click(); //click on dropdown
    cy.get('[data-value="Ontology"]').click(); //clicks on Real estate
    cy.get('#filled-basic').should('be.visible'); //verifies Commit SHA field
    cy.get('[data-cy=comment_field]').should('be.visible'); //verifies Import reason field
    cy.get('[data-cy=import-button]').click(); // click Import button

    cy.get('#responsive-dialog-title'); //verifes that responsive dialog is shown
    cy.get('[data-cy="close"]').click(); //close the dialog
    //specific responsive messages are to be implemented
  });
});
