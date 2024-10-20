# Prerequisites

- [k6](https://k6.io/docs/getting-started/installation)
- [NodeJS](https://nodejs.org/en/download/)
- [Yarn](https://yarnpkg.com/getting-started/install) (optional)

**Install dependencies**

```bash
$ yarn install
```

# Transpiling and Bundling

By default, k6 can only run ES5.1 JavaScript code. To use TypeScript, we have to set up a bundler that converts TypeScript to JavaScript code. 

This project uses `Babel` and `Webpack` to bundle the different files - using the configuration of the [`webpack.config.js`](./webpack.config.js) file.

If you want to learn more, check out [Bundling node modules in k6](https://k6.io/docs/using-k6/modules#bundling-node-modules).

# Running the test and test environment

Existing tests are written to execute against the `PortalXL` service deployed on the `UAT` instance. 

To run a test written in TypeScript, we first have to transpile the TypeScript code into JavaScript and bundle the project, using the "yarn webpack" command.

```bash
$ yarn webpack
```

This command converts the typescript scripts located in subfolders in the `./src` folder to javascript scripts located in the `./dist` folder.

When running in the pipeline, a job called `k6-build` runs `yarn webpack` (and other jobs) automatically when changes are made to the main branch.

Once that is done, the js scripts are ready to run.

# Writing own tests

House rules for writing tests:
- The test code is located in individual folders in the `./src/` folder
- The entry points for the individual tests (or single k6 api calls) need to have `*test*.ts` in the name to distinguish them from auxiliary files. You can change the entry [here](./webpack.config.js#L8). This creates js files that enable you to run the individual test cases. 
- If they don't have `*test*.ts` in the name, the script `./webpack.config.js` will not transpile them from typescript to javascript, and they will not run individually. However, if the script paths are added to index.ts, the scripts can be run as part of a scenario even if `*test*.ts` does not appear in their name. 
- If static files are required then add them to `./assets` folder. The contents of the `./assets` folder gets copied to the `./dist` folder along with the compiled scripts.
- The files in the `./assets` folder will be in the same folder where the scripts run from (`./dist`), and can therefore can be referenced in the scripts with `./`

# Running tests from local machine

This repo consists of: 
1. `Scenario` based tests - tests that model diverse traffic patterns made up by calling one or more `individual` tests.
2. `Individual` tests - single user tests which can be executed independently. These may or may not be used to define a `Scenario`.

`Scenario` tests make use of key `scenarios` as shown below:
```
export const options = {
  scenarios: {
    example_scenario: { }
    }
}
```
Running tests require the following environment variables to run - speak with someone from `QA` to get appropriate values:
1. `API_BASEURL`
2. `CLIENT_ID`
3. `SCOPE`
4. `USER_DEFAULT_NAME` & `USER_DEFAULT_PASSWORD` (could be passed as ENV variables for individual or scenario tests, or in a `users.json` file for scenario tests)

If a username and password are not passed as `env` variables, a file called `users.json` needs to be created. The file should be added to the `assets` folder in the project root. 

The file contents should be a `json` array as below:
```
[
    { "username": "qa+au_ad@willowinc.com", "password": "***" }
]
```
Refer to `users-template.json` as an example.

## Running scenario tests locally

The `scenario` tests will look for a `USER_FILE` either specified as an environment variable at run time, i.e. `--env USER_FILE=...` or in the `./dist/users.json` file, which was copied from the `./assets` folder during the yarn webpack build.

One of these files need to exist for the tests to run.

If the `--env USER_FILE=...` or `./dist/users.json` file has missing or `null` values for user credentials, the script will fallback on CLI environment variables `--env USER_DEFAULT_NAME and --env USER_DEFAULT_PASSWORD`. If a valid usernmane and password is not found in any of these locations, the tests will fail with authentication errors (no username or password).

_Example command runs when a valid username and password is in the users.json file located in the assets folder:_

>yarn webpack && k6 run dist/first-load-test.js --env API_BASEURL=https://wil-uat-plt-aue1-portalxl.azurewebsites.net --env SCOPE="openid 24368ed6-d050-4852-9f79-ef3838e6e8e5" --env CLIENT_ID=24368ed6-d050-4852-9f79-ef3838e6e8e5

**Note: The `scenario` tests will not run and `k6` will throw a file not found exception if `./dist/users.json` file does NOT exist, or you have not explicitly specified an alternative location to a users file as an environment variable ('--env USER_FILE=...'). The code expects a `users.json` or '--env USER_FILE=...' to be available when running `Scenario` tests.** 

## Running individual tests locally

For `individual` single user tests a user needs to be supplied using CLI environment variables `USER_DEFAULT_NAME` and `USER_DEFAULT_PASSWORD`. The users for `individual` tests are NOT loaded from the `./dist/users.json` file. 

_Example command:_

>yarn webpack && k6 run dist/get-200-sites-all-test.js --env API_BASEURL=https://command-uat.willowinc.com/au/api/ --env CLIENT_ID=24368ed6-d050-4852-9f79-ef3838e6e8e5 --env SCOPE="openid 24368ed6-d050-4852-9f79-ef3838e6e8e5" --env USER_DEFAULT_NAME=qa+au_ad@willowinc.com --env USER_DEFAULT_PASSWORD=***

# Test runs in pipeline
These load tests are currently run on a schedule in the pipeline. The test uses login credentials which are downloaded as a secure file (stored in the pipeline library) during the pipeline runs (`load-test-users.json`). The format of the file matches what is specified in `user-template.json`.

**Note: If more test users are to be added to the secure file, or it needs to be updated, the existing file has to be deleted and a new one uploaded with the same file name.**  

# Reports

Result metrics from scheduled pipeline runs is available for analysis in this [Grafana dashboard](https://grafana.devservices.willowinc.com/d/XKhgaUpik/k6-load-testing-results-by-groups?orgId=1&refresh=1m)
