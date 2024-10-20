#!/bin/bash

# This script is executed after `npm install` (it's referenced as the `prepare`
# script in `package.json`. Sets up config blame.ignoreRevsFile and installs
# our Prettier pre-commit hook.
if [ "$CI" != "true" ]; then
  git config blame.ignoreRevsFile .git-blame-ignore-revs
  cd ../../../../
  dotnet tool restore
  husky install
fi
