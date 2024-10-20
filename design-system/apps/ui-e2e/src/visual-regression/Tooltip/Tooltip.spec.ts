import { test, expect } from '@willowinc/playwright'

const componentName = 'Tooltip'
const groupName = 'Overlays'

test('default tooltip', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  await page.getByRole('button').hover()

  await expect(page.getByRole('tooltip')).toHaveScreenshot()
})

test('customized width tooltip', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Multiline')

  await page.getByRole('button').hover()

  await expect(page.getByRole('tooltip')).toHaveScreenshot()
})

test('trigger on focus', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'TriggerOnFocus')

  await page.getByRole('button').focus()
  await expect(page.getByRole('tooltip')).toBeVisible()

  await page.getByRole('button').blur()
  await expect(page.getByRole('tooltip')).toBeHidden()
})
