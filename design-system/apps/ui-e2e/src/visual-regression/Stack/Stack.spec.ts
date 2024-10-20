import { test, expect } from '@willowinc/playwright'

const componentName = 'Stack'
const groupName = 'Layout'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('gap', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Gap')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('justify', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Justify')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('align', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Align')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
