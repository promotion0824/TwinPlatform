import { test, expect } from '@willowinc/playwright'

const componentName = 'Error'

test('default error', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'Default')

  await expect(storybook.storyContainer).toHaveScreenshot()
})
