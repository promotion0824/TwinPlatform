import { test, expect } from '@willowinc/playwright'

const componentName = 'SidePanel'

test.describe('SidePanel', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 500, height: 200 })
  })

  test('default SidePanel', async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'DefaultSidePanel')

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('collapsed SidePanel', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'DefaultSidePanel')
    await page.getByRole('button', { name: 'collapse' }).click()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('customized width with ellipsis title', async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'CustomizedWidth')

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('collapsed SidePanel with customized width', async ({
    storybook,
    page,
  }) => {
    await storybook.gotoHidden(componentName, 'CustomizedWidth')
    await page.getByRole('button', { name: 'collapse' }).click()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('overflowed content', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'OverflowContent')

    const lastElement = page.getByText('Last Dummy content')
    await expect(lastElement).not.toBeInViewport()

    // element was hidden, and can scroll to element means the panel is scrollable
    await lastElement.scrollIntoViewIfNeeded()
    await expect(lastElement).toBeInViewport()
  })
})
