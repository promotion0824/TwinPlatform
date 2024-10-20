import { test, expect } from '@willowinc/playwright'

const componentName = 'LineChart'
const groupName = 'Charts'

test('line chart', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('multiple lines', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'MultipleLines')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('smoothed', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Smoothed')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('time series', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TimeSeries')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
