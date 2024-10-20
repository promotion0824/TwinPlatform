/// <reference types="cypress" />
import './commands';
// ***********************************************************
// This example plugins/index.js can be used to load plugins
//
// You can change the location of this file or turn off loading
// the plugins file with the 'pluginsFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/plugins-guide
// ***********************************************************

// This function is called when a project is opened or re-opened (e.g. due to
// the project's config changing)
const fs = require('fs-extra');
const path = require('path');

const fetchConfigurationByFile = (file) => {
  const pathOfConfigurationFile = `config/cypress.${file}.json`;

  return file && fs.readJson(path.join(__dirname, '../', pathOfConfigurationFile));
};

module.exports = (_on, _config) => {
  const environment = config.env.configFile || 'test';
  const configurationForEnvironment = fetchConfigurationByFile(environment);

  return configurationForEnvironment || config;
};
//@type {Cypress.PluginConfig}
/* 
// eslint-disable-next-line no-unused-vars
module.exports = (_on, _config) => {
  // `on` is used to hook into various events Cypress emits
  // `config` is the resolved Cypress config
};
// cypress/plugins/index.js
*/