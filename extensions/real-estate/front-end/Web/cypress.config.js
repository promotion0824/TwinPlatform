const { defineConfig } = require('cypress')

module.exports = defineConfig({
  defaultCommandTimeout: 30000,
  chromeWebSecurity: false,
  viewportWidth: 1920,
  viewportHeight: 1080,
  reporter: 'junit',
  reporterOptions: {
    mochaFile: './cypress/results/output-[hash].xml',
  },
  e2e: {
    // We've imported your old cypress plugins here.
    // You may want to clean this up later by importing these.
    setupNodeEvents(on, config) {
      return require('./cypress/plugins/index.js')(on, config)
    },
    experimentalSessionAndOrigin: true,
    baseUrl: 'http://localhost:8080',
    excludeSpecPattern: 'cypress/e2e/pvt/*',
  },
})
