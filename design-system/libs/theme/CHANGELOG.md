# @willowinc/theme

## 1.0.0-alpha.63

## 1.0.0-alpha.62

## 1.0.0-alpha.61

## 1.0.0-alpha.60

## 1.0.0-alpha.59

## 1.0.0-alpha.58

### Patch Changes

- **General**: [`ce4e20e56754357d452cc050dad5ed8db237e1d3`](https://github.com/WillowInc/TwinPlatform/commit/ce4e20e56754357d452cc050dad5ed8db237e1d3) Upgrade nx to 19
- **General**: [`842a36b5c587aaabb01f76694b308aa1554ac96d`](https://github.com/WillowInc/TwinPlatform/commit/842a36b5c587aaabb01f76694b308aa1554ac96d) add focus style globally
- **General**: [`52c48f50f4f7e70d6c40934ccac046c9ddf98635`](https://github.com/WillowInc/TwinPlatform/commit/52c48f50f4f7e70d6c40934ccac046c9ddf98635) adjust neutral background colours for better colour contrast with text

## 1.0.0-alpha.57

## 1.0.0-alpha.56

### Minor Changes

- **General**: [`e332a3860d`](https://github.com/WillowInc/TwinPlatform/commit/e332a3860d) Build theme using Style Dictionary and export pre-built theme

## 1.0.0-alpha.55

### Minor Changes

- **General**: [`ab8bc77265`](https://github.com/WillowInc/TwinPlatform/commit/ab8bc77265) update global scrollbar styling

## 1.0.0-alpha.54

## 1.0.0-alpha.53

### Patch Changes

- **General**: [`591192087a`](https://github.com/WillowInc/TwinPlatform/commit/591192087a) Update Playwright tests for Tabs
- **General**: [`4fe608ab65`](https://github.com/WillowInc/TwinPlatform/commit/4fe608ab65) Export pre-built palettes
  Use Design Tokens format for palette

## 1.0.0-alpha.52

## 1.0.0-alpha.51

## 1.0.0-alpha.50

### Minor Changes

- **General**: [`55ed044074`](https://github.com/WillowInc/TwinPlatform/commit/55ed044074) add global breakpoints

## 1.0.0-alpha.49

## 1.0.0-alpha.48

## 1.0.0-alpha.47

## 1.0.0-alpha.46

### Minor Changes

- **Breaking change**: [`64c3ce8566`](https://github.com/WillowInc/TwinPlatform/commit/64c3ce8566) upgrade styled-components from 5 to 6

  - You might need to install 'babel-plugin-styled-components' if not already installed;
  - You might need to uninstall '@types/styled-components' if already installed;
  - You need to update styles that are using pseudo selectors like ':hover'.
    See this doc for details https://willow.atlassian.net/wiki/x/BYCSow

## 1.0.0-alpha.45

### Patch Changes

- **chore**: [`0e5961359c`](https://github.com/WillowInc/TwinPlatform/commit/0e5961359c) fix spell to US English and add setting for it

## 1.0.0-alpha.44

## 1.0.0-alpha.43

## 1.0.0-alpha.42

## 1.0.0-alpha.41

## 1.0.0-alpha.40

## 1.0.0-alpha.39

## 1.0.0-alpha.38

## 1.0.0-alpha.37

## 1.0.0-alpha.36

## 1.0.0-alpha.35

## 1.0.0-alpha.34

## 1.0.0-alpha.33

## 1.0.0-alpha.32

## 1.0.0-alpha.31

## 1.0.0-alpha.30

## 1.0.0-alpha.29

## 1.0.0-alpha.28

## 1.0.0-alpha.27

## 1.0.0-alpha.26

## 1.0.0-alpha.25

## 1.0.0-alpha.24

## 1.0.0-alpha.23

### Minor Changes

- **General**: [`aa22bed875`](https://github.com/WillowInc/TwinPlatform/commit/aa22bed875) "create mvp of copilot widget"

## 1.0.0-alpha.22

### Patch Changes

- **bugfix**: [`5721a36721`](https://github.com/WillowInc/TwinPlatform/commit/5721a36721) fix npm error with pre release script [#81804](https://dev.azure.com/willowdev/Unified/_workitems/edit/81804/)
- **tool**: [`b06f955aa8`](https://github.com/WillowInc/TwinPlatform/commit/b06f955aa8) enable esm support for our packages

## 1.0.0-alpha.21

### Patch Changes

- Updated dependencies
  - [@willowinc/palette@1.0.0-alpha.21](/docs/release-notes-palette--docs#100-alpha21)

## 1.0.0-alpha.20

### Minor Changes

- **theme**: [`35fe591103`](https://github.com/WillowInc/TwinPlatform/commit/35fe591103) change export of theme provider to export from ui package, and move global style for Mantine to ui package.

### Patch Changes

- **ui**: [`ab7e0a5b73`](https://github.com/WillowInc/TwinPlatform/commit/ab7e0a5b73) update global style for Mantine
- Updated dependencies
  - [@willowinc/palette@1.0.0-alpha.20](/docs/release-notes-palette--docs#100-alpha20)

## 1.0.0-alpha.19

### Minor Changes

- **UI**: [`3f9a40c574`](https://github.com/WillowInc/TwinPlatform/commit/3f9a40c574) Migrate TextInput

### Patch Changes

- Updated dependencies
  - [@willowinc/palette@1.0.0-alpha.19](/docs/release-notes-palette--docs#100-alpha19)

## 1.0.0-alpha.18

### Major Changes

- **theme-token!**: [`0ff1bb743c`](https://github.com/WillowInc/TwinPlatform/commit/0ff1bb743c) Update font tokens to different font weight for different body and display type [#80017](https://dev.azure.com/willowdev/Unified/_workitems/edit/80017/)

  BREAKING CHANGE:

  ```
  theme.font.body.xs => theme.font.body.xs.regular
  theme.font.body.sm => theme.font.body.sm.regular
  theme.font.body.md => theme.font.body.md.regular
  theme.font.body.lg => theme.font.body.lg.regular

  theme.font.display.sm => theme.font.display.sm.regular
  theme.font.display.md => theme.font.display.md.regular
  theme.font.display.lg => theme.font.display.lg.regular
  ```

### Minor Changes

- **General**: [`818713beff`](https://github.com/WillowInc/TwinPlatform/commit/818713beff) Migrate Select to Mantine and add visual tets

### Patch Changes

- **Bugfix**: [`bc76cffa76`](https://github.com/WillowInc/TwinPlatform/commit/bc76cffa76) fix errors when releasing alpha.18. - include font file for material-symbols into bundle. - reexport Checkbox and Textarea and mark as deprecated.
- Updated dependencies
  - [@willowinc/palette@1.0.0-alpha.18](/docs/release-notes-palette--docs#100-alpha18)

## 1.0.0-alpha.17

### Patch Changes

- **refactor**: [`f9b4927560`](https://github.com/WillowInc/TwinPlatform/commit/f9b4927560) export fontWeight as number in Theme
- Updated dependencies [`5d5572adc9`](https://github.com/WillowInc/TwinPlatform/commit/5d5572adc9)
  - [@willowinc/palette@1.0.0-alpha.17](/docs/release-notes-palette--docs#100-alpha17)

## 1.0.0-alpha.16

### Patch Changes

- **release-tool**: [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec) Added release tool and updated contribution docs [#78110](https://dev.azure.com/willowdev/Unified/_workitems/edit/78110/)
- **bug**: [`2fdc160c89`](https://github.com/WillowInc/TwinPlatform/commit/2fdc160c89) move css import level to solve random style overrides in Storybook
- **release-tool**: [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec) Updated commands and documentation [#78795](https://dev.azure.com/willowdev/Unified/_workitems/edit/78795/)
- **build**: [`dd2ad7dcae`](https://github.com/WillowInc/TwinPlatform/commit/dd2ad7dcae) Ensure build doesn't bundle any external modules [#79632](https://dev.azure.com/willowdev/Unified/_workitems/edit/79632/)
- **chore**: [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec) remove unwanted folder layer
- **docs**: [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec) Updated README and added known issues page
- Updated dependencies [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec), [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec), [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec), [`670da11dec`](https://github.com/WillowInc/TwinPlatform/commit/670da11dec)
  - [@willowinc/palette@1.0.0-alpha.16](/docs/release-notes-palette--docs#100-alpha16)
