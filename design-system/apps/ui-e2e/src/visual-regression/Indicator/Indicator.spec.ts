import { test, expect } from '@willowinc/playwright'

const componentName = 'Indicator'
const groupName = 'Data Display'

test('intents', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Intents')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('label', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Label')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('has border', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'HasBorder')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('position', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Position')

  await expect(storybook.storyContainer).toHaveScreenshot()
})
