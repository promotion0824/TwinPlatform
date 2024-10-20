/// <reference types="cypress" />
describe('Testing Models deletion', () => {
  before('Login and get all tokens ', () => {
      cy.Login();
  });
    
  it('checks for DELETE confirmation and deletes successfully', function () {
    cy.visit('/');
    cy.get('[data-cy="delete-models"]').should('include.text', 'Delete Models').click(); //click on Delete Models
    cy.get('[data-cy="comment"]').type('comment'); //type comment
    cy.get('[data-cy="delete-twins"]').should('include.text', 'Delete Models').click(); //click Delete Models

    // The Proceed button is disabled as Confirmation is not entered
    cy.get('[data-cy="proceed-button"]').should('have.attr', 'disabled');

    // Enter a confirmation message which is NOT DELETE and check Proceed is still disabled
    cy.get('[data-cy="confirmation"]').type('test');
    cy.get('[data-cy="proceed-button"]').should('have.attr', 'disabled');

    // Clear the previously entered text and enter correct confirmation message DELETE and proceed with deletion
    cy.get('[data-cy="confirmation"]').clear();
    cy.get('[data-cy="confirmation"]').type('DELETE'); //type DELETE in the confirmaton field
    cy.get('[data-cy="proceed-button"]').click(); //click Proceed
    //should have a Good Confirmation Message
  });
});
