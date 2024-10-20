import { test, expect } from '@willowinc/playwright'

const componentName = 'SearchInput'
const groupName = 'Inputs'

test('placeholder', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('default value', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'WithDefaultValue')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('with label and description', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'WithLabelAndDescription')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('disabled', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')

  await expect(storybook.storyRoot).toHaveScreenshot()
})
