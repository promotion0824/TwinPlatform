import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'TagsInput'
const groupName = 'Inputs'

const getInput = async (page: Page) => page.locator('.mantine-TagsInput-root')
const getDropdown = async (page: Page) =>
  page.locator('.mantine-TagsInput-dropdown')
const openDropdown = async (page: Page) => page.getByRole('textbox').click()

test.use({
  viewport: { width: 210, height: 100 },
})

test('Default', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'playground')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('With multiple values and placeholder', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'WithValues')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('With placeholder', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'withPlaceholder')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('Readonly with value', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'readonlyWithValue')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('Disabled with value', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'disabledWithValue')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('Readonly with placeholder', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'readonlyWithPlaceholder')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('Disabled with placeholder', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'disabledWithPlaceholder')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('With label and error message', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithLabelAndErrorMessage')

  await expect(await getInput(page)).toHaveScreenshot()
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test.describe('With suggestions', () => {
  test.use({
    viewport: { width: 210, height: 200 },
  })

  test.beforeEach(async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'WithSuggestions')
    await openDropdown(page)
  })

  test('dropdown area', async ({ page }) => {
    await expect(await getDropdown(page)).toHaveScreenshot()
  })

  test('dropdown option on hover', async ({ page }) => {
    const appleOption = page.getByRole('option', {
      name: 'Apple',
    })
    await appleOption.hover()

    await expect(appleOption).toHaveScreenshot()
  })

  test('dropdown option will be removed from list after selected', async ({
    page,
  }) => {
    expect(await page.getByRole('option').all()).toHaveLength(3)
    await page.getByRole('option').first().click()

    expect(await page.getByRole('option').all()).toHaveLength(2)
  })
})
