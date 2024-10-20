import { test, expect } from '@willowinc/playwright'

const componentName = 'Label'

test('default label', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'Default')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('required label', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'Required')

  await expect(storybook.storyContainer).toHaveScreenshot()
})
