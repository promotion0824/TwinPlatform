const { defineConfig } = require('cypress');

module.exports = defineConfig({
  execTimeout: 18000,
  defaultCommandTimeout: 300000,
  requestTimeout: 10000,
  pageLoadTimeout: 30000,
  responseTimeout: 10000,
  viewportWidth: 1200,
  viewportHeight: 1200,
  chromeWebSecurity: false,
  retries: {
    runMode: 1,
    openMode: 2,
  },
    e2e: {
    experimentalSessionAndOrigin:true,
        baseUrl: 'https://localhost:44423/',
        setupNodeEvents(on, config) {
            require("cypress-localstorage-commands/plugin")(on, config);
            return config;
        },
  },
});
