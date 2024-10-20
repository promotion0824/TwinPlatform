import { test, expect } from '@willowinc/playwright'

const componentName = 'Progress'
const groupName = 'Feedback'

test('intents', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Intents')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('sizes', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Sizes')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
