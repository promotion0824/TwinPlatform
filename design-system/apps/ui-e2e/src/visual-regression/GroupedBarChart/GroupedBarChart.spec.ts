import { test, expect } from '@willowinc/playwright'

const componentName = 'GroupedBarChart'
const groupName = 'Charts'

test('row chart', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('column chart', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ColumnChart')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('row chart with positive and negative values', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'PositiveAndNegativeValues')
  await expect(page.getByTestId('row-positive-and-negative')).toHaveScreenshot()
})

test('column chart with positive and negative values', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'PositiveAndNegativeValues')
  await expect(
    page.getByTestId('column-positive-and-negative')
  ).toHaveScreenshot()
})

test('row chart with negatives', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NegativeValues')
  await expect(page.getByTestId('row-negative')).toHaveScreenshot()
})

test('column chart with negatives', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NegativeValues')
  await expect(page.getByTestId('column-negative')).toHaveScreenshot()
})

test('line overlay', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'LineOverlay')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('time series', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TimeSeries')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
