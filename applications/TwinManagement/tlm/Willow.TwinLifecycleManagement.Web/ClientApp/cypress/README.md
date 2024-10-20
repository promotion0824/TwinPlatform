# Cypress UI tests

## Before running tests

Navigate to the root of `TwinPlatform\extensions\tlm\Willow.TwinLifecycleManagement.Web\ClientApp>` repo and run the follwing command:

```
npm install
npm install cypress
npm i cypress-file-upload --save-dev
```

## Providing api token manually

- Navigate to login page and login with your credentials
- Click F12 and navigate to Console tab
- Focus console like you intend to type something in it
- Token script required for the next step can be found on this [link](https://willow.atlassian.net/wiki/spaces/WCP/pages/edit-v2/2195194210)
- Paste code from the token script in the console and press enter
- Pop up message will be shown: Code with token data is copied into your clipboard
- Navigate to the commented test script inside ClientApp\cypress\support>commands.js, and past token data from clipboard right after `Cypress.Commands.add('InjectToken', () => {`

## Running tests on local instance of application

Start the Client side locally:

- Simply run `npm start`.
- Make sure local client side is running on `http://localhost:44423/`.
- Then run the following command to start test execution.

```
npm run cypress -- --env email="email",password="password"
```

## Choose test to run
