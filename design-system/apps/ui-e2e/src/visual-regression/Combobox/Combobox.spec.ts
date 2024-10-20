import { test, expect } from '@willowinc/playwright'

const componentName = 'Combobox'

test.describe('Combobox sub components', async () => {
  test.beforeEach(async ({ storybook, page }) => {
    await page.setViewportSize({ width: 300, height: 300 })
    await storybook.gotoHidden(componentName, 'ComboboxSimpleExample')
  })

  test('InputBase', async ({ page }) => {
    await expect(
      page.getByRole('button', { name: 'Pick value' })
    ).toHaveScreenshot()
  })

  test('InputPlaceholder', async ({ page }) => {
    await expect(page.getByText('Pick value')).toHaveScreenshot()
  })

  test('Dropdown', async ({ page }) => {
    await expect(page.getByRole('presentation')).toHaveScreenshot()
  })

  test('Options', async ({ page }) => {
    await expect(page.getByRole('listbox')).toHaveScreenshot()
  })

  test('Option', async ({ page }) => {
    const optionElement = page.getByRole('option', { name: 'Apples' })
    await optionElement.hover()

    await expect(optionElement).toHaveScreenshot()
  })

  test('Header', async ({ page }) => {
    await expect(page.getByText('Test Header')).toHaveScreenshot()
  })

  test('Footer', async ({ page }) => {
    await expect(page.getByText('Test Footer')).toHaveScreenshot()
  })
})

test.describe('Combobox sub components', async () => {
  test.beforeEach(async ({ storybook, page }) => {
    await page.setViewportSize({ width: 300, height: 300 })
    await storybook.gotoHidden(componentName, 'ComboboxWithSelectedValue')
  })

  test('Input value', async ({ page }) => {
    await expect(
      page.getByRole('button', { name: 'Bananas' })
    ).toHaveScreenshot()
  })

  test('Option disabled', async ({ page }) => {
    const optionElement = page.getByRole('option', { name: 'Chocolate' })
    await optionElement.hover()

    await expect(optionElement).toHaveScreenshot()
  })

  test('Option selected', async ({ page }) => {
    await expect(
      page.getByRole('option', { name: 'Bananas' })
    ).toHaveScreenshot()
  })

  test('Search', async ({ page }) => {
    await expect(page.getByRole('textbox')).toHaveScreenshot()
  })
})

test('Empty', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 300, height: 300 })
  await storybook.gotoHidden(componentName, 'ComboboxWithEmptyOption')

  await expect(page.getByText('No options')).toHaveScreenshot()
})
