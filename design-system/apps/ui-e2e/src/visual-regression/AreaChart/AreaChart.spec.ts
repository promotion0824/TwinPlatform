import { test, expect } from '@willowinc/playwright'

const componentName = 'AreaChart'
const groupName = 'Charts'

// TODO: Make 100% consistent for this test
// test('area chart', async ({ storybook }) => {
//   await storybook.goto(componentName, groupName, 'Playground')
//   await expect(storybook.storyRoot).toHaveScreenshot()
// })

test('smoothed', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Smoothed')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('multiple datasets', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'MultipleDatasets')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('line overlay', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'LineOverlay')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('time series', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TimeSeries')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
