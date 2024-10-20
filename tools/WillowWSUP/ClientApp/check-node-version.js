import semver from 'semver';
// Ref --> https://nodejs.org/api/packages.html
//import pkg from './package.json'assert { type: "json" };
import { createRequire } from "module";
const require = createRequire(import.meta.url);
const pkg = require('./package.json');

// Per the npm docs the preinstall script, which is where this js file is ran, does not actually run before dependencies are run after actual installation of modules into node_modules
// See --> https://docs.npmjs.com/cli/v10/using-npm/scripts#life-cycle-operation-order
const version = pkg.engines.node;
if (!semver.satisfies(process.version, version)) {
  console.log(
    `Required node version ${version} not satisfied with current version ${process.version}.`,
  );
  process.exit(1);
}
