import { expect, test } from '@willowinc/playwright'

const componentName = 'Loader'
const groupName = 'Feedback'

test('intents', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Intents')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('ovals', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Oval')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('dots', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Dots')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

// Cannot test bars because of the animation cannot be paused by Playwright,
// also won't work with `{animations: 'allow'}` option
