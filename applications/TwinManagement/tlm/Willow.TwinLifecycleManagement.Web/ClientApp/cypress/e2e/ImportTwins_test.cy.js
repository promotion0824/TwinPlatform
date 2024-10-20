/// <reference types="cypress" />


describe('Testing Import Twins', () => {
    before('Login and get all tokens ', () => {
        cy.LoginLS();
        cy.saveLocalStorage('AfterLogin');
    });
    beforeEach('Inject tokens and navigate to Jobs Page', () => {
        cy.restoreLocalStorage('AfterLogin');
        cy.visit("/");
        cy.get('[data-cy="menu-wrapper"]').find(':contains("Import")').click();
        cy.get('[data-cy="import-twins"]').click();
    });

    it.skip('Checks Twins Import - Validate Default Values ', function (){
        cy.get('[data-cy="upload-button"]').should('include.text', 'Upload');
        cy.get('#filled-basic').invoke('val').should('be.empty');
        cy.get('[data-cy="include-relationships-checkBox"]>input').should('be.checked');
        cy.get('[data-cy="include-twin-properties-checkBox"]>input').should('be.checked');
        cy.get('[data-cy="delete-twins-file"]').should('be.disabled');
    });
    it.skip('Checks Twins Import with csv file - Include relationships Yes, Include Twin Properties Yes,Correct Location ', function () {
        populateImportTwinsData("TwinsImportcsv.csv", "", true, true, "Test");
        cy.get('[data-cy="delete-twins-file"]').should('not.be.disabled').click();
        cy.get('[data-cy="refresh"]').should('be.disabled');
        cy.get('[data-cy="AllJobs"]').should('be.visible');
  });
    it.skip('Checks Twins Import with xls file - Include relationships Yes, Include Twin Properties Yes,Empty Location,With Comment', function (){

        populateImportTwinsData("TwinsImportxls.xlsx", "", true, true, "Test Comment");
        cy.get('[data-cy="delete-twins-file"]').should('not.be.disabled').click();
        cy.get('[data-cy="refresh"]').should('be.disabled');
        cy.get('[data-cy="AllJobs"]').should('be.visible');
    });
    it('Checks Twins Import - No Twins/models location', function (){

        populateImportTwinsData("TwinsImportcsv.csv", "non-existing location", true, false, "");
        cy.get('[data-cy="delete-twins-file"]').should('not.be.disabled').click();

        cy.get('[data-cy="show-details-button"]').should('be.visible').click();
        cy.get('[data-cy="error-message"]').should('be.visible').contains('Status code: 424');
        cy.get('[data-cy="close-button"]').should('be.visible').click();

    });
    it('Checks Twins Import - Empty File', function () {

        populateImportTwinsData("emptyFile.xlsx", "", false, true, "");
        cy.get('[data-cy="delete-twins-file"]').should('not.be.disabled').click();

        cy.get('[data-cy="show-details-button"]').should('be.visible').click();
        cy.get('[data-cy="error-message"]').should('be.visible').contains('Status code: 424');
        cy.get('[data-cy="close-button"]').should('be.visible').click();
    });
    it('Checks Twins Import - Corrupted File', function (){

        populateImportTwinsData("HVACValve.csv", "", false, true, "corrupted file");
        cy.get('[data-cy="delete-twins-file"]').should('not.be.disabled').click();

        cy.get('[data-cy="show-details-button"]').should('be.visible').click();
        cy.get('[data-cy="error-message"]').should('be.visible').contains('Status code: 424');
        cy.get('[data-cy="close-button"]').should('be.visible').click();

    });
    
});


function populateImportTwinsData(file, location, includeRelationships, includeTwinProperties, comment){
    cy.get('[data-cy="upload-button"]').should('include.text', 'Upload');
    //HVACValve.csv attach file for import
    cy.get('#contained-button-file').attachFile(file);
    //clear location and enter new 
    if (location!=="") cy.get('#filled-basic').clear().type(location);
    //check checkboxes and check/uncheck if needed
    cy.get('[data-cy="include-relationships-checkBox"]>input')
        .then(($checkbox) => {
            includeRelationships == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
    cy.get('[data-cy="include-twin-properties-checkBox"]>input')
        .then(($checkbox) => {
            includeTwinProperties == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
    if (comment!=="") cy.get('[data-cy="comment"]').click().clear().type(comment); //click on Import twin button
}
