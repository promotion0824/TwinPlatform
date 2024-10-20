import { test, expect } from '@willowinc/playwright'

const componentName = 'ChartCard'
const groupName = 'Charts'

test('default (showing stacked bar chart)', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'StackedBar')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('data table', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'StackedBar')
  await page.getByRole('button').click()
  await page.getByText('View Data').click()
  await expect(storybook.storyRoot).toHaveScreenshot()
})
