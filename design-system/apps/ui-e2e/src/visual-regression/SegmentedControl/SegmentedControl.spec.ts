import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'SegmentedControl'
const groupName = 'Inputs'

const getByLabel = (page: Page, label: string) =>
  page.locator('label').filter({ hasText: label })
test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('control status', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'DisabledControl')

  const disabledControl = getByLabel(page, 'visibilityPreview')
  await storybook.testInteractions(
    'disabled-control',
    disabledControl,
    disabledControl,
    ['default', 'hover']
  )

  const selectedControl = getByLabel(page, 'codecode')
  await storybook.testInteractions(
    'selected-control',
    selectedControl,
    selectedControl,
    ['default', 'hover']
  )

  const defaultControl = getByLabel(page, 'export')
  await storybook.testInteractions(
    'default-control',
    defaultControl,
    defaultControl,
    ['default', 'hover']
  )
})

test('sizing for icon only', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'IconOnly')

  const visibilityControl = getByLabel(page, 'visibility')

  const element = await visibilityControl.boundingBox()

  // it's flaky because it will show text instead of icon if material-symbols font
  // is not loaded, which causes the size get expanded by the text. wait for font loaded
  // does not work here. So just try to retry and wait longer for the assertion
  await expect(async () => {
    expect(element?.width).toBe(24)
  }).toPass({
    timeout: 8000,
  })
  expect(element?.height).toBe(24)
})

test('icon only', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'IconOnly')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('with prefix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'WithPrefix')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('vertical', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Vertical')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('full width', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'FullWidth')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
