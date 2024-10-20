import { test, expect } from '@willowinc/playwright'

const componentName = 'Sparkline'
const groupName = 'Charts'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('fill', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Fill')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
