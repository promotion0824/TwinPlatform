import { test, expect } from '@willowinc/playwright'

const componentName = 'ChartTable'
const groupName = 'Charts'

test('chart table', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('progress column', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ProgressColumn')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('link column', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'LinkColumn')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
