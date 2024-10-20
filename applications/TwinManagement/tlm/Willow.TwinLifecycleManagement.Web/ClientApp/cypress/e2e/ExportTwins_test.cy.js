/// <reference types="cypress" />
const downloadsFolder = Cypress.config('downloadsFolder')
describe('Testing Export Twins', () => {

    before('Login and navigate to Export twins page', () => {
        Cypress.config('baseUrl', 'https://localhost:7071');
        cy.LoginLS();
        cy.saveLocalStorage('AfterLogin');
    });
    after('Clear and logout ', () => {
        Cypress.session.clearAllSavedSessions();
    });
   
    beforeEach('Clear location input', () => {
        //cy.Login();
        //cy.navigateToExportTwinsPage();
        //cy.get('[data-cy="ETLocationInput"]').should('be.visible').click().clear();
        cy.restoreLocalStorage('AfterLogin');
        cy.navigateToExportTwinsPage();
    });

    it('CHECK - empty models id - relationships false - location incorrect ', function () {
        cy.intercept({
            method: 'POST',
            url: '/Export/twins?locationId=NonExistingLocation&exactModelMatch=true&includeRelationships=false&includeIncomingRelationships=false'
        }).as('ETIncorrectLocation');
        cy.get('[data-cy="ETIncludeRelationships"]').should('be.visible').click();
        cy.get('[data-cy="ETLocationInput"]').click().clear().type("NonExistingLocation");
        cy.get('[data-cy="ETbutton"]').should('not.be.disabled').click();

        cy.validateDownloadedFile('@ETIncorrectLocation', 'POST', 404, downloadsFolder)
    });

    it('CHECK - incorrect models id - relationships false - location empty ', function () {
        cy.get('[data-cy="ETModelNames"]').click().clear().type("NonExistingModel");
        cy.get('[data-cy="ETLocationInput"]').click();
        cy.get('[data-cy="ETIncludeRelationships"]').should('be.visible').click();
        cy.get('[data-cy="ETModelNames"]').invoke('val').should('be.empty')
    });

    it('CHECK - models id empty  - relationships true - location empty', function () {

        cy.intercept({
            method: 'POST',
            url: '/Export/twins?locationId=&exactModelMatch=true&includeRelationships=true&includeIncomingRelationships=false'
        }).as('ETEmptyFields');

        cy.get('[data-cy="ETbutton"]').should('not.be.disabled').click();
        cy.validateDownloadedFile('@ETEmptyFields', 'POST', 200, downloadsFolder);
    });

    it('CHECK - one model - relationships true - location empty ', function () {

        cy.intercept({
            method: 'POST',
            url: '/Export/twins?locationId=&exactModelMatch=true&includeRelationships=true&includeIncomingRelationships=false'
        }).as('ETemptyLocationValidModelRelationshipsTrue');

        cy.get('#models_ids').click();
        cy.contains('li', 'Busway').click();
        cy.get('[data-cy="ETbutton"]').should('not.be.disabled').click();
        cy.clearSelectedModels();

        cy.validateDownloadedFile('@ETemptyLocationValidModelRelationshipsTrue', 'POST', 200, downloadsFolder);
    });

    it('CHECK - two models - relationships true - location empty ', function () {

        cy.intercept({
            method: 'POST',
            url: '/Export/twins?locationId=&exactModelMatch=true&includeRelationships=true&includeIncomingRelationships=false'
        }).as('TwoModelsEmptyLocation');

        cy.get('#models_ids').click();
        cy.contains('li', 'Busway').click();
        cy.get('#models_ids').type('Binary');
        cy.contains('li', 'Binary Actuator').click();
        cy.get('[data-cy="ETbutton"]').should('not.be.disabled').click();
        cy.clearSelectedModels();

        cy.validateDownloadedFile('@TwoModelsEmptyLocation', 'POST', 200, downloadsFolder);
    });

    it('CHECK - one model - relationships true - location correct ', function () {

        cy.intercept({
            method: 'POST',
            url: '/Export/twins?locationId=4e5fc229-ffd9-462a-882b-16b4a63b2a8a&exactModelMatch=true&includeRelationships=true&includeIncomingRelationships=false'
        }).as('ETCorrectLocation');

        cy.get('[data-cy="ETModelNames"]').click();
        cy.get('[data-cy="ETLocationInput"]').click().type('4e5fc229-ffd9-462a-882b-16b4a63b2a8a')
        cy.get('#models_ids', { timeout: 15000 }).should('be.visible').click();
        cy.contains('li', 'Busway').click();
        cy.get('[data-cy="ETbutton"]').should('not.be.disabled').click();
        cy.clearSelectedModels();

        cy.validateDownloadedFile('@ETCorrectLocation', 'POST', 200, downloadsFolder);
    });

    it('CHECK - Model with no twins- relationships true - location empty ', function () {

        cy.intercept({
            method: 'POST',
            url: '/Export/twins?locationId=&exactModelMatch=true&includeRelationships=true&includeIncomingRelationships=false'
        }).as('ETModelWithNoTwin');

        cy.get('#models_ids').click();
        cy.contains('li', 'Binary Actuator').click();
        cy.get('[data-cy="ETbutton"]').should('not.be.disabled').click();
        cy.clearSelectedModels();

        cy.validateDownloadedFile('@ETModelWithNoTwin', 'POST', 404, downloadsFolder);
        })
    });
