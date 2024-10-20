import { test, expect } from '@willowinc/playwright'

const componentName = 'Field'

test.beforeEach(async ({ page }) => {
  await page.setViewportSize({
    width: 284 /* this weird width is to meet the snapshot width in v6 */,
    height: 400,
  })
})

test('with label', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'WithLabel')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('with error', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'WithError')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('required with error', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'RequiredWithError')

  await expect(storybook.storyContainer).toHaveScreenshot()
})
