import { test, expect } from '@willowinc/playwright'

const componentName = 'BarChart'
const groupName = 'Charts'

test('row chart', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('column chart', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ColumnChart')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('row chart with negatives', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NegativeValues')
  await expect(page.getByTestId('row-with-negatives')).toHaveScreenshot()
})

test('column chart with negatives', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NegativeValues')
  await expect(page.getByTestId('column-with-negatives')).toHaveScreenshot()
})

test('line overlay', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'LineOverlay')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('time series', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TimeSeries')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('willow-intent theme', async ({ storybook }) => {
  await storybook.goto('chart-themes', groupName, 'IntentThresholds')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
