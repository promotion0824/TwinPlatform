import { test, expect, Page } from '@willowinc/playwright'

const openDrawer = (page: Page) =>
  page.getByRole('button', { name: 'open drawer' }).click()

// These tests use toBeCloseTo as they can often return values that are
// extremely close (such as 520.0000610351562 instead of 520), but not
// exact matches.

const componentName = 'Drawer'
const groupName = 'Overlays'

test('default size small', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')
  await page.setViewportSize({ width: 400, height: 200 })

  await openDrawer(page)
  await expect(page.getByRole('dialog')).toHaveScreenshot('default-size.png')

  const box = await page.getByRole('dialog').boundingBox()
  expect(box?.width).toBeCloseTo(380)
})

test('size middle', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'MdSize')
  await page.setViewportSize({ width: 600, height: 200 })

  await openDrawer(page)
  const box = await page.getByRole('dialog').boundingBox()
  expect(box?.width).toBeCloseTo(440)
})

test('size large', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'LgSize')
  await page.setViewportSize({ width: 1000, height: 200 })

  await openDrawer(page)

  const box = await page.getByRole('dialog').boundingBox()
  expect(box?.width).toBeCloseTo(520, 0)
})

test('size extraLarge', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'SizeExtraLarge')
  await page.setViewportSize({ width: 800, height: 200 })

  await openDrawer(page)

  const box = await page.getByRole('dialog').boundingBox()
  expect(box?.width).toBeCloseTo(780)
})

test('size fullScreen', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'FullScreen')
  const pageWidth = 250
  await page.setViewportSize({ width: pageWidth, height: 200 })

  await openDrawer(page)

  const box = await page.getByRole('dialog').boundingBox()
  expect(box?.width).toBeCloseTo(pageWidth)
})
