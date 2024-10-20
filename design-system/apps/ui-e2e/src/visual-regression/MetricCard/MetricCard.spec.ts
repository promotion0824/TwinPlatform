import { test, expect } from '@willowinc/playwright'

const componentName = 'MetricCard'
const groupName = 'Charts'

test('positive sentiment', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})

test('negative sentiment', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NegativeSentiment')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})

test('notice sentiment', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NoticeSentiment')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})

test('neutral sentiment', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'NeutralSentiment')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})

test('units', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Units')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})

test('description', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Description')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})

test('without thousands separator', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Without Thousands Separator')
  await expect(page.getByTestId('metric-card-decorator')).toHaveScreenshot()
})
