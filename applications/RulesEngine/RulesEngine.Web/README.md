# Willow Packages

How to get started using our libraries in a react application:

Npm uses the .npmrc file located in the project's directory (if present) to determine the authentication token and other configurations.

- [Setup]

Per-user NPM config location

MacOS: use path `~/.npmrc` to find the file.
Windows: you can find the file in `C:\Users\{your user name}\.npmrc`.
Add the following lines:
______________________________________________________________________

@willowinc:registry=https://npm.pkg.github.com/
//npm.pkg.github.com/:_authToken={YOUR PAT TOKEN without the brackets}
______________________________________________________________________

See below how to get your own PAT token or see https://storybook-dev.willowinc.com/?path=/docs/getting-started-developers--docs

Before importing a library into an application you need to configure npm with the permissions required to read or write packages to GitHub.

To get started, add a personal access token to your user .npmrc file:

Go to the Personal access tokens (classic) page in your GitHub account (https://github.com/settings/tokens)
Select Generate new token -> Generate new token (classic)
Give your token a name and expiration period
Select read:packages to be able to install a package into an app
Generate the token and take a copy of the value
Back on the tokens page in GitHub select Configure SSO for your token and authorize the token for the WillowInc organisation
Edit the .npmrc file in your home directory, adding these two lines:

You will now be able to install and use @willow packages
