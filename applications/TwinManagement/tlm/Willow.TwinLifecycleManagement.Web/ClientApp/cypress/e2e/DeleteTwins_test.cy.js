/// <reference types="cypress" />
describe('Testing Delete twins', () => {
  before('Login and get all tokens ', () => {
    cy.Login();
  });

  //Delete All Twins
  it('Tests Delete All Twins', function () {
    cy.visit('/'); //visits Homepage
    cy.get('#composition-button').should('include.text', 'Delete Twins').click(); //click on Delete Twins
    cy.get('[data-cy="delete-all-twins"]').click(); //click on Delete All Twins from dropdown
    cy.get('[data-cy="delete-twins"]').click(); //clicks on Delete Twins button

    //new dialog opens, checks if Proceed button is disabled
    cy.get('[data-cy="proceed-button"]').should('have.attr', 'disabled');

    // Enter a confirmation message which is NOT DELETE and check Proceed is still disabled
    cy.get('[data-cy="confirmation"]').type('test');
    cy.get('[data-cy="proceed-button"]').should('have.attr', 'disabled');

    // Clear the previously entered text and enter correct confirmation message DELETE and proceed with deletion
    cy.get('[data-cy="confirmation"]').clear();
    cy.get('[data-cy="confirmation"]').type('DELETE'); //type DELETE in the confirmaton field
    cy.get('[data-cy="cancel-button"]').click(); //click Cancel, dialog closes

    cy.get('[data-cy="delete-twins"]').click(); //clicks on Delete Twins button again
    cy.get('[data-cy="confirmation"]').type('DELETE'); //type DELETE in the confirmaton field
    cy.get('[data-cy="proceed-button"]').click(); //click Proceed

    cy.get('#responsive-dialog-title'); //verifes that responsive dialog is shown
    cy.get('[data-cy="close"]').click(); //close the dialog
    //specific responsive messages are to be implemented
  });

  //Delete Twins with Site ID
  it('Delete Twins By Site Id  - With site id', function () {
    cy.visit('/'); //visits Homepage
    cy.get('#composition-button').should('include.text', 'Delete Twins').click(); //click on Delete Twins
    cy.get('[data-cy="delete-twins-siteid-dropdown"]').click(); //click on Delete Twins with a Site ID from dropdown
    cy.get('#filled-basic').type(123); //type Site ID
    cy.get('[data-cy="comment-siteID"]').type('comment'); //type comment
    cy.get('[data-cy="delete-twins-siteid"]').click(); //click Delete Twins

    // Enter a confirmation message which is NOT DELETE and check Proceed is still disabled
    cy.get('[data-cy="confirmationID"]').type('test');
    cy.get('[data-cy="proceed-button-siteid"]').should('have.attr', 'disabled');

    // Clear the previously entered text and enter correct confirmation message DELETE,Proceed button shall be visible
    cy.get('[data-cy="confirmationID"]').clear();
    cy.get('[data-cy="confirmationID"]').type('DELETE'); //type DELETE in the confirmaton field

    //cancel the action
    cy.get('[data-cy="cancel-siteID"]').click(); //click Cancel

    //no confirmation check
    cy.get('[data-cy="delete-twins-siteid"]').click(); //click Delete Twins
    cy.get('[data-cy="proceed-button-siteid"]').should('have.attr', 'disabled'); //checks if Proceed button is disabled

    cy.get('[data-cy="confirmationID"]').type('DELETE'); //type DELETE in the confirmaton field
    cy.get('[data-cy="proceed-button-siteid"]').click(); //click Proceed
    //Bug 70440: Can Delete twins without entering required site ID

    cy.get('#responsive-dialog-title'); //verifes that responsive dialog is shown
    cy.get('[data-cy="close"]').click(); //close the dialog
    //specific responsive messages are to be implemented
  });

  //Delete Twins from a file
  it('Tests Delete Twins From File', function () {
    cy.visit('/'); //visits Homepage
    cy.get('#composition-button').should('include.text', 'Delete Twins').click(); //click on Delete Twins
    cy.get('[data-cy="delete-twins-from-file"]').click(); //click on Delete Twins from a file, from dropdown

    //tests if the Delete button is disabled before requred data is entered
    cy.get('[data-cy="delete-twins-file"]').should('have.attr', 'disabled');

    cy.get('#contained-button-file').attachFile('HVACPumpGroup.xlsx'); //upload file/
    cy.get('[data-cy="siteId"]').type(123); //type Site ID

    cy.get('[data-cy="comment"]').type('comment'); //type comment
    cy.get('[data-cy="checkBox"]').click(); // check the box - Include relationships in the import process
    cy.get('[data-cy="delete-twins-file"]').click(); //click Delete

    //new dialog appears
    cy.get('#responsive-dialog-title'); //verifes that responsive dialog is shown
    cy.get('[data-cy="close"]').click(); //close the dialog
    //specific responsive messages are to be implemented
  });
});
