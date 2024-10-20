# willow-web

Contains Willow platform frontend projects.

![logo]

## Before installation

You must use a personal access token (classic) to authenticate to
GitHub Packages before downloading the Willow's UI packages.
See [Developers guide to get started](../../../../design-system/libs/ui/src/docs/getting-started/developers.stories.mdx#authentication)

Steps to get and authenticate with your GitHub's Personal Access Token (classic) - Reference

1. Go to the "Personal access tokens (classic)" page in your GitHub account (https://github.com/settings/tokens)
2. Select Generate new token -> Generate new token (classic)
3. Give your token a name and expiration period
4. Select `read:packages` to be able to install a package into an app
5. Generate the token and take a copy of the value
6. Back on the tokens page in GitHub select Configure SSO for your token and authorize the token for the WillowInc organisation
7. Edit the .npmrc your home directory (`~/.npmrc`), adding this line:

```
//npm.pkg.github.com/:_authToken=TOKEN
```

## Installation

```
$ npm install
$ npm start
```

## Packages

- `@willow/platform`
  - Willow Platform website
- `@willow/mobile`
  - Willow Mobile website
- `@willow/ui`
  - Shared components between Platform and Admin
- `@willow/campus`
  - Shared component library for campus view to be used across different Willow vertical platforms
- `woggle`
  - Chrome extension that shows Platform feature toggles

## Commands

- `npm start`
  - Starts the `Willow Platform` dev server at http://localhost:8080 with `apiUrl` set based on the https://ddk.app.willowinc.com
- `npm run platform-other`
  - Starts the `Willow Platform` dev server at http://localhost:8080 with `apiUrl` set based on user's selection
- `npm run mobile`
  - starts the `Willow Mobile` dev server at http://localhost:8082 pointing to uat

### ArcGIS proxy

If you want to test ArcGIS locally, you can use a local proxy to get around the fact that
localhost is not enabled in the CORS settings on DFW's ArcGIS server.

See [this page](https://willow.atlassian.net/wiki/spaces/MAR/pages/2369454285/Getting+mapsonline.dfwairport.com+working+locally)
for details on setting up the proxy.

---

- `npm run lint`
  - runs js and css linting over all projects

---

- `npm run test`
  - runs unit tests over all projects
- `npm run type-check`
  - runs type check over all typescript files

---

- `npm run build`
  - builds a production `Willow Platform` build into `./packages/platform/dist`
- `npm run build-mobile`
  - builds a production `Willow Mobile` build into `./packages/mobile/dist`

## URLs of live environments

### Platform

Login to https://wsup.willowinc.com/ to view the full list of apps and their url under "Willow App".

### Mobile

- UAT: https://wil-uat-lda-shr-glb-command-mobile.azurefd.net
- Production: https://command-mobile.willowinc.com

Note: these URLs are also listed on Confluence:
https://willow.atlassian.net/wiki/spaces/MAR/pages/1983905857/URLs

## Credentials

Ask your manager or the security team if you do not have access.

## Module aliases

You will notice that we can import from `@willow/ui` and these imports resolve
to files in the `packages/ui/src` directory. This is currently managed both in
`babel.config.js` and `tsconfig.json`, so be sure to keep these in sync.

## Mock server

The frontend primarily talks to a server called PlatformPortalXL ([repo here] [platformportalxl]).

We use [Mock Service Worker] [msw] to mock it.

This makes it easy for us to develop the frontend against backend routes that do not exist yet.

The mock server mocks the routes we tell it to mock. See
`packages/platform/src/mockServer.js`. Other routes will hit the real services
as normal.

The mock server is disabled by default. To use it, start your frontend process with one of the following commands:

- `npm run platform-mock` (will fall back to UAT)
- `npm run platform-mock-dev` (will fall back to Dev)
- `npm run platform-mock-prod` (will fall back to Prod)
- `npm run platform-mock-local` (will fall back to local)

Note that these commands differ from their counterparts without `-mock` only in
that they set the `MOCK_SERVICE_WORKER` environment variable to `"true"`.

You will need to restart your frontend in order to enable or disable the mock server.

[Initial writeup / rationale for Mock Service Worker](https://willow.atlassian.net/wiki/spaces/~918887516/pages/1997078532/Mocking)

## Branching

This branching strategy for this repo is using [trunk based development](https://trunkbaseddevelopment.com/). Any branches are short lived, and code that isn't to be released immediately is managed in code with feature toggles.

Frontend feature toggles are available by adding a cookie (woggle makes this a little easier)
e.g. `wp-some-feature-enabled=true` (following the format `wp-{something}-enabled` or `wp-{something}-disabled`)
and can be accessed like:

```js
const config = useConfig()

if (config.hasFeatureToggle('wp-some-feature-enabled')) {
  return <SomeFeature />
}
```

There are "customer" level feature toggles returned inside `/api/me`, and "site" level feature toggles returns inside `/api/me/sites`.

## Development

### JS

- the folder structure is feature based, keeping `.js`, `.css` together as required, and in general not referencing files "outside" of the current folder, only in the current folder and subfolders

### CSS

- CSS variables are inside `/packages/ui/src/theme.css`
- colors/fonts/padding/z-indexes etc. used throughout should be using values set here by default, and only using other values in exceptions
- CSS modules is used by default so CSS is local by default
- import any css files last inside .js files to ensure ordering of the prod generated .css file is correct
