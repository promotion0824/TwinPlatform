// To fix css prop type error that introduced by using StoryContainer in stories.
// See https://github.com/DefinitelyTyped/DefinitelyTyped/issues/31245#issuecomment-446011384
// Import {} will pass the type check for ui-e2e.
// But need to import * instead of {} to be able to run tests in Playwright.

// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { CSSProp } from 'styled-components'

export * from '@playwright/test'

export { test } from './fixtures'
