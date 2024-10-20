import { test, expect } from '@willowinc/playwright'

const componentName = 'Modal'
const groupName = 'Overlays'

test('default modal', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('without header area', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithoutHeaderArea')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('without close button', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithoutCloseButton')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('only close button', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'OnlyCloseButton')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('vertically centered', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'VerticallyCentered')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 300 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('small size', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'SmallSize')

  await page.getByRole('button').click()
  await expect(page.getByText('Modal Content')).toHaveScreenshot()
})

test('medium size', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'SizeMd')

  await page.getByRole('button').click()
  await expect(page.getByText('Modal Content')).toHaveScreenshot()
})

test('large size', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'SizeLg')

  await page.getByRole('button').click()
  await expect(page.getByText('Modal Content')).toHaveScreenshot()
})

test('extra large size', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'ExtraLargeSize')

  await page.getByRole('button').click()
  await expect(page.getByText('Modal Content')).toHaveScreenshot()
})

test('percentage size', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'PercentageSize')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 500, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('auto size', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'AutoSize')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})

test('full screen', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'FullScreen')
  // set window size smaller, so the snapshot is smaller
  await page.setViewportSize({ width: 400, height: 200 })

  await page.getByRole('button').click()
  await expect(page).toHaveScreenshot()
})
