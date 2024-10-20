import { test, expect } from '@willowinc/playwright'

const componentName = 'TimeInput'
const groupName = 'Dates'

test('default TimeInput with fullWidth', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')

  const input = page.getByRole('textbox')
  await storybook.testInteractions('default-input.png', input, input, [
    'default',
    'focus',
    'click',
  ])
})

test('readyOnly input', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Readonly')
  // set window size smaller, so the snapshot width is smaller
  await page.setViewportSize({ width: 200, height: 400 })

  await expect(page.getByRole('textbox')).toHaveScreenshot()
})

test('disabled input', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, ' Disabled')
  // set window size smaller, so the snapshot width is smaller
  await page.setViewportSize({ width: 200, height: 400 })

  await expect(page.getByRole('textbox')).toHaveScreenshot()
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('input with label and error', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'HiddenWithLabelAndError')

  const labelAndInput = page.getByText('LabelError Message')
  await expect(labelAndInput).toHaveScreenshot()
})

test.describe('default TimeInput', () => {
  test.beforeEach(async ({ page }) => {
    // set window size smaller, so the snapshot width is smaller
    await page.setViewportSize({ width: 200, height: 400 })
  })

  test('dropdown list', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'Default')
    await page.getByRole('textbox').click()

    await expect(page.getByRole('dialog')).toHaveScreenshot()
  })

  test('input and dropdown list', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'Default')
    await page.getByRole('textbox').click()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })
})

test.describe('with seconds format', () => {
  test.beforeEach(async ({ storybook, page }) => {
    // set window size smaller, so the snapshot width is smaller
    await page.setViewportSize({ width: 200, height: 400 })
    await storybook.goto(componentName, groupName, 'WithSecondsFormat')
  })
  test('dropdown list with seconds format', async ({ page }) => {
    await page.getByRole('textbox').click()

    await expect(page.getByRole('dialog')).toHaveScreenshot()
  })

  test('selected value with seconds format', async ({ page }) => {
    const input = page.getByRole('textbox')
    await input.click()

    await page.getByRole('button', { name: '08:45:00 am' }).click()
    await expect(input).toHaveScreenshot()
  })
})

test.describe('with 24 hours format and seconds', () => {
  test.beforeEach(async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'HiddenIn24HoursWithSeconds')
  })

  test('dropdown list', async ({ page }) => {
    await page.getByRole('textbox').click()

    await expect(page.getByRole('dialog')).toHaveScreenshot()
  })

  test('selected value', async ({ page }) => {
    const input = page.getByRole('textbox')
    await input.click()

    await page.getByRole('button', { name: '08:45:00' }).click()
    await expect(input).toHaveScreenshot()
  })
})

test('dropdown list with 1 hr interval', async ({ storybook, page }) => {
  // set window size smaller, so the snapshot width is smaller
  await page.setViewportSize({ width: 200, height: 400 })
  await storybook.goto(componentName, groupName, 'With1HourInterval')
  await page.getByRole('textbox').click()

  await expect(page.getByRole('dialog')).toHaveScreenshot()
})
