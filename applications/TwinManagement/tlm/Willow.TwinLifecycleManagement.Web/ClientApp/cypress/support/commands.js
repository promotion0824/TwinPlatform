/// <reference types="cypress" />

import 'cypress-file-upload';

Cypress.Commands.add('Login', () => {
    cy.session('LoginSession', () => {

        cy.visit("/");
        cy.get('[data-cy="login-button"]').click();
        cy.get('#signInName').type(Cypress.env("email"));
        cy.get('#continue').click();
        cy.get('#password').type(Cypress.env("password"));
        cy.get('#next').click();
        cy.get('[data-cy="home-button"]').should('be.visible');

        //Navigate to jobs page to get all tokens in local storage
        cy.wait(2000);
        cy.get('[data-cy="import-jobs"]').click();
        cy.wait(2000);
    });
});
Cypress.Commands.add('LoginLS', () => {
        cy.visit("/");
    cy.get('[data-cy="login-button"]').click();
    const sentArgs = { username: Cypress.env("email"), password: Cypress.env("password") };
    cy.origin('https://willowdevb2c.b2clogin.com/willowdevb2c.onmicrosoft.com', { args: sentArgs }, ({ username, password }) => {
        cy.get('#signInName').type(username);
        cy.get('#continue').click();
        cy.get('#password').type(password);
        cy.get('#next').click();
    });
        cy.get('[data-cy="home-button"]').should('be.visible');
        //Navigate to jobs page to get all tokens in local storage
         cy.wait(2000);
        cy.get('[data-cy="import-jobs"]').click();
        cy.wait(2000);
});

Cypress.Commands.add('Logout', () => {
    cy.visit('/');
    cy.get('[data-cy="log-out-button"]').click();
    cy.get('[data-cy="home-button"]').should('not.be.visible');
    cy.get('[data-cy="login-button"]').should('be.visible');
});

Cypress.Commands.add('goBackToHome', () => {
  cy.get('[data-cy=home-button]').click(); //goes back to Homepage
  cy.get('p').should(
    'include.text',
    'Twin Lifecycle Management is there to help you import your Twins and Models to your environment.' //verifying Homepage
    );
});

//Import Twins
Cypress.Commands.add('navigateToImportTwinsPage', () => {
   
});

//Import Models
Cypress.Commands.add('navigateToImportModelsPage', () => {
    cy.visit("/");
  cy.get('a[data-cy="import-models"]').click();
});

//Jobs
Cypress.Commands.add('navigateToJobsPage', () => {
  cy.visit("/");
  cy.get('a[data-cy=import-jobs]').should('include.text', 'Jobs').click(); //click on Jobs
});

//ExportTwins
Cypress.Commands.add('navigateToExportTwinsPage', () => {
    cy.visit("/");
    cy.get('[data-cy="export-twins"]').click();
});

//DataQuality
Cypress.Commands.add('navigateToDataQualityPage', () => {
    cy.get('[data-cy="data-quality"]').click();
});

//Models
Cypress.Commands.add('navigateToModelsPage', () => {
    cy.get('[data-cy="models"]').click();
});

Cypress.Commands.add('validateDownloadedFile', (routeName, method, statusCode, downloadsFolder) => {
    cy.wait(routeName, { timeout: 50000 }).then(xhr => {
        expect(xhr.request.method).to.eq(method);
        expect(xhr.response.statusCode).to.eq(statusCode);
        cy.log(xhr.requestBody);
        if (statusCode === 200) {
            let fileName = xhr.response.headers["content-disposition"].split(";");
            fileName = fileName[1].split("=");
            fileName = fileName[1];
            fileName = downloadsFolder + "\\" + fileName;
            cy.log(fileName);
            cy.readFile(fileName).should('exist');
        } else if (statusCode === 404) {

            cy.get('[data-cy="error-message-title"]').should('be.visible').contains('Hmm, that was not found at all');
            cy.get('[data-cy="show-details-button"]').should('be.visible').click();
            cy.get('[data-cy="error-message"]').should('be.visible').contains('Status code: 404');
            cy.get('[data-cy="close-button"]').should('be.visible').click();
        }
    });
});

Cypress.Commands.add('clearSelectedModels', () => {
    cy.get('#models_ids').click();
    cy.get('[data-testid="CloseIcon"]').click();
    cy.get('[data-cy="ETLocationInput"]').click();
});
Cypress.Commands.add('NavigateToJobsPage', () => {
    cy.visit('/');
    cy.get('a[data-cy=import-jobs]').should('include.text', 'Jobs').click(); //click on Jobs
});
Cypress.Commands.add('SetJobFilter', (Done, Error, Processing, Queued, Canceled) => {
    cy.get('.PrivateSwitchBase-input.css-1m9pwf3')
        .filter('[value="Done"]')
        .then(($checkbox) => {
            Done == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
    cy.get('.PrivateSwitchBase-input.css-1m9pwf3')
        .filter('[value="Error"]')
        .then(($checkbox) => {
            Error == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
    cy.get('.PrivateSwitchBase-input.css-1m9pwf3')
        .filter('[value="Processing"]')
        .then(($checkbox) => {
            Processing == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
    cy.get('.PrivateSwitchBase-input.css-1m9pwf3')
        .filter('[value="Queued"]')
        .then(($checkbox) => {
            Queued == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
    cy.get('.PrivateSwitchBase-input.css-1m9pwf3')
        .filter('[value="Canceled"]')
        .then(($checkbox) => {
            Canceled == true ? cy.wrap($checkbox).check() : cy.wrap($checkbox).uncheck();
        });
});

