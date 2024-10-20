import { test, expect } from '@willowinc/playwright'

const componentName = 'FileList'
const groupName = 'feedback'

test.beforeEach(async ({ page }) => {
  await page.setViewportSize({ width: 300, height: 300 })
})

test('succeed FileList', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Succeed')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('loading FileList', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Loading')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('disabled FileList', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('failed FileList', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Failed')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('with border for FileList', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'WithBorder')

  await expect(storybook.storyRoot).toHaveScreenshot()
})
