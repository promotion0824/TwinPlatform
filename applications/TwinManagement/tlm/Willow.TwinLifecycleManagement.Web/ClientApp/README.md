# Willow.TwinLifecycleManagement.Web Client App

This is the front-end for Twin Lifecycle Management extension developed with React and TypeScript. It provides a user interface to the twin and model management functionality.

## Before installation

You must use a personal access token (classic) to authenticate to
GitHub Packages before downloading the Willow's UI packages.
See [Developers guide to get started](https://storybook.willowinc.com/1.0.0-alpha.22/?path=/docs/getting-started-developers--docs#authentication)

Steps to get and authenticate with your GitHub's Personal Access Token (classic) - Reference

1. Go to the "Personal access tokens (classic)" page in your GitHub account (https://github.com/settings/tokens)
2. Select Generate new token -> Generate new token (classic)
3. Select `read:packages` to be able to install a package into an app
4. Generate the token and take a copy of the value
5. On the tokens page in GitHub select "Configure SSO" for your token and authorize the token for the WillowInc organisation
6. Edit the .npmrc your home directory (`/.npmrc`), adding this line:

```
//npm.pkg.github.com/:_authToken=TOKEN
```

## Local startup

```
npm i
npm start
```

## Formatting codebase

A front-end part of the TLM codebase is formatted using package called "Prettier", so all the code follow the same format pattern. For easier formatting the code new script is created in application.json, under "Scripts" section with the name: "Format". Script to format all code in the solution is: `npm run format`.

## Running tests

### UI Tests

UI tests are run through Crypress and are located at the `./cypress` folder.

Note that Cypress tests currently cannot be run automatically due to limitations caused by security policy.
See [Cypress Tests README](./cypress/README.md) for more information on running these tests manually.

## Deployment

Please consult the [Twin Lifecycle Management README file](../README.md) for information on how to deploy the extension.
