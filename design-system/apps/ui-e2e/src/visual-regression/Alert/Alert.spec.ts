import { test, expect } from '@willowinc/playwright'

const componentName = 'Alert'
const groupName = 'Feedback'

test('Default Alert', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'playground')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('Variants', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Variants')
  const stories = storybook.storyRoot.getByTestId('alert')

  const intents = ['primary', 'secondary', 'positive', 'negative', 'notice']

  for (let i = 0; i < intents.length; i++)
    await expect(stories.nth(i)).toHaveScreenshot(`${intents[i]}-alert.png`)
})

test('With Close Button', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'WithCloseButton')

  await expect(storybook.storyRoot).toHaveScreenshot()
})
