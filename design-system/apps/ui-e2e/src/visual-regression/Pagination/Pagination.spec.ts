import { test, expect } from '@willowinc/playwright'

const componentName = 'Pagination'
const groupName = 'Navigation'

test('default', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')
  await page.setViewportSize({ width: 800, height: 30 })
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('first breakpoint (no item count)', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')
  await page.setViewportSize({ width: 600, height: 30 })
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('second breakpoint (no page number selector)', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'playground')
  await page.setViewportSize({ width: 450, height: 30 })
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('third breakpoint (no items per page label)', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'playground')
  await page.setViewportSize({ width: 280, height: 30 })
  await expect(storybook.storyRoot).toHaveScreenshot()
})
