# Woggle 2

This is a browser extension that lets us override the values of any ConfigCat feature flag,
just for the current session, without having to log into ConfigCat and without affecting any
other users.

This extension based on a boilerplate repo: https://github.com/Jonghakseo/chrome-extension-boilerplate-react-vite

**Note**: there is something funny in the build system here. If we move the `woggle-2` directory
inside the `Web` directory, we will get weird import errors when we try to build. That is why
the `woggle-2` directory is where it is.

### Installation

#### Build:

```
cd extensions/real-estate/front-end/woggle-2
npm install
npm run build
```

If you want to develop Woggle 2, you can use `npm run dev` instead of `npm run build`,
and then you will get hot reload for the extension.

#### Install in Chrome or Edge:

1. Open in browser: `chrome://extensions`
2. Make sure the "Developer mode" checkbox is checked
3. Click "Load unpacked"
4. Select the `extensions/real-estate/front-end/woggle-2/dist` directory

### Usage

1. Open the Willow app (production, UAT, local, single tenant, it doesn't matter).
2. Click the extension icon to open the popup
3. Find the features you want in the list and set the values you want
4. _Refresh the Willow app when done_

When you want to reset, there's a "Reset all" button on the popup.

### How does it work?

In the Willow app we add an item to local storage under the key `configCatSettings`,
with the following type:

```
{
  baseSettings: Array<{settingKey: string, settingValue: boolean}>,
  overrideSettings: {[settingKey: string]: boolean]}
}
```

The Willow app calls ConfigCat to determine the "base settings" - the user's
actual ConfigCat settings in the absence of any overrides, and writes it to
local storage.

Woggle reads the `baseSettings`, but uses it only to display the list of
settings and their base values in its UI. Woggle's primary task is to set the
value of `overrideSettings`. When the user updates a setting in Woggle, it is
written back to `overrideSettings` in local storage in the main app. _It is up
to the user to then refresh the page._
