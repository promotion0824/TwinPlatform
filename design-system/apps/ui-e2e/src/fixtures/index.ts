import { test as base } from '@playwright/test'
import { Storybook } from './storybook'

export const test = base.extend<{
  /**
   * Fixture to provide methods to interact with Storybook
   *
   * **Example**
   *
   * ```js
   * import { test } from '@willowinc/playwright';
   *
   * test('basic test', async ({ storybook }) => {
   *   // go to button playground story
   *   await page.goto('button', 'playground');
   *   // go to button document page
   *   await page.goto('button');
   *   // ...
   * });
   * ```
   */
  storybook: Storybook
}>({
  storybook: async ({ page }, use) => {
    await use(new Storybook(page))
  },
})
