import { test, expect } from '@willowinc/playwright'

const componentName = 'Tracker'
const groupName = 'Charts'

test('intent variant', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('status variant', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'StatusVariant')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('label and description', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'LabelAndDescription')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('tooltip label', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'TooltipLabels')
  await page
    .getByTestId('tooltip-labels-tracker')
    .locator('div:nth-child(3)')
    .hover()
  await expect(page.getByRole('tooltip')).toHaveScreenshot()
})
