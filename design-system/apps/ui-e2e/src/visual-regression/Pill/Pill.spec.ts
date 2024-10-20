import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'Pill'
const groupName = 'data-display'

const getPill = (page: Page, label = 'Label') =>
  page.locator('span').filter({ hasText: label }).first()

test('Pill default style', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')

  await expect(getPill(page)).toHaveScreenshot()
})

test('Pill with remove button', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'removable')

  await expect(getPill(page)).toHaveScreenshot()
})

test('RemovalButton should be hidden when Pill is disabled', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(componentName, 'DisabledWithRemovalButton')

  await expect(getPill(page)).toHaveScreenshot()
})

test('Pill with medium size', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'SizeMd')

  await expect(getPill(page)).toHaveScreenshot()
})
