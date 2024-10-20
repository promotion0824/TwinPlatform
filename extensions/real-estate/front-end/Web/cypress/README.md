# Cypress UI tests

## 1. Before running tests

Navigate to the root of `/Web` repo and run the follwing command:

```
npm install
```

At the moment tests are run on **Google Chrome browser** by default.

## 2. Running tests against local instance of application

1. Make sure local client side is running on `http://localhost:8080`. (Run local application pointing to `UAT` data - `npm run platform`)
2. Then run the following command to start test execution.

_This command runs tests using default `cypress.config.js` file. By default test execution from CLI runs in headless mode. Pass option `--headed` in the command if you want to disable `headless` mode._

```
npm run cypress-run-local -- --env email=qa+au_ca@willowinc.com,password=***
```

**Note:**

_The password for test accounts are stored in LastPass. Please get in touch with a QA team member so they can securely share the password with you via LastPass. This is a user with role `Customer Admin` being used in the tests._

## 3. Running tests against UAT

1. Run the following command to start test execution on UAT instance:

```
npm run cypress-run-uat -- --env email=qa+au_ca@willowinc.com,password=***
```

## 4. Running tests using Cypress App (The Launchpad)

To run the tests from Cypress Test Runner (Which allows you to see commands as they execute) run the following command:

```
npm run cypress-local -- --env email=qa+au_ca@willowinc.com,password=***
```

This will open the interactive runner to run the test `spec` files against local instance of application.

To run the tests against `UAT`, run command `npm run cypress-uat` and pass the `email, password` credentials as shown above.

## 5. Running tests in Release Pipeline

Currently, the E2E tests are run in release pipeline against `UAT` test environment. For more details on how the pipeline stage is configured refer to this [Confluence page](https://willow.atlassian.net/l/cp/1E1X1fxG)

## 6. Running PVT (Product Validation Testing) tests on Willow Demo site

There are a set of tests which are planned to run during the PVT phase of testing on a staging environment.

These tests run against the `Willow Demo` customer in `AU` region.

The tests are located in `/e2e/pvt` folder. The config `cypress.pvt.config.js` is specific to `pvt` tests and will only run tests under the `/e2e/pvt` folder.

These tests will be configured to run in the continuous delivery pipeline. The following command is used to start the tests (in Cypress run mode):

```
npm run cypress-run-uat -- --env email=qa+willow_au_prd_admin@willowinc.com,password=*** --config-file ./cypress/cypress.pvt.config.js
```

**Note:**

_The password for PVT test account is stored in LastPass. Please get in touch with a QA team member so they can securely share the password with you via LastPass. This is a user with role `Customer Admin` being used in the tests._

## 7. Running tests on Ephemeral Environments

Cypress tests can be run from your local machine pointing to an ephemeral environment. Once the ephemeral environment is up and running, the following command is used to start Cypress tests (using the Cypress app in interactive mode):

```
npm run cypress-ephemeral -- --env email=qa+au_ca@willowinc.com,password=*** --config baseUrl=https://<env-name>.nonprod.willowinc.com
```

**Note:**

_As ephemeral environments point to a copy of data taken from `UAT-AU` instance, therefore we need to use the same login credentials that are used to run tests on `UAT`._
