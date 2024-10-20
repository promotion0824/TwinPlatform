import { test, expect } from '@willowinc/playwright'

const componentName = 'RingProgress'
const groupName = 'Feedback'

test('intents', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Intents')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('icon', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Icon')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('show value', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ShowValue')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
test('sizes', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'sizes')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
