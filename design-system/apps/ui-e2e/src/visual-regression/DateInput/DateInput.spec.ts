import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'DateInput'
const groupName = 'Dates'

test.beforeEach(async ({ page }) => {
  await page.setViewportSize({ width: 300, height: 350 })
})

const openCalendar = async (page: Page) => {
  await page.getByRole('textbox').click()
}

test.describe('DateInput', () => {
  test('default placeholder', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'Playground')

    const input = page.getByPlaceholder('Date input placeholder')
    await expect(input).toBeVisible()
    await expect(input).toHaveScreenshot()
  })

  test('prefix and suffix', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'PrefixAndSuffix')

    await expect(page.getByRole('textbox')).toHaveScreenshot()
  })

  test('required with label and error', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'RequiredWithLabelAndError')

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

  test('readonly', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'readonly')

    await expect(page.getByRole('textbox')).toHaveScreenshot()
  })

  test('disabled', async ({ storybook, page }) => {
    await storybook.goto(componentName, groupName, 'disabled')

    await expect(page.getByRole('textbox')).toHaveScreenshot()
  })

  test('with calendar open', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDefaultDate')

    await openCalendar(page)

    await expect(page).toHaveScreenshot()
  })

  test('clear button hovered background not bigger than input box', async ({
    storybook,
    page,
  }) => {
    await storybook.goto(componentName, groupName, 'Clearable')

    const clearButton = page.getByRole('button', { name: 'close' })
    await clearButton.hover()

    await expect(page.getByRole('textbox')).toHaveScreenshot()
  })
})

test.describe('date cell in calendar', () => {
  test('normal date', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDefaultDate')
    await openCalendar(page)

    const normalDate = page.getByRole('button', {
      name: '4 January 2023',
      exact: true,
    })
    await storybook.testInteractions('normal-date', normalDate, normalDate, [
      'default',
      'hover',
    ])
  })

  test('outside date', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDefaultDate')
    await openCalendar(page)

    const outsideDate = page.getByRole('button', {
      name: '29 December 2022',
      exact: true,
    })

    await storybook.testInteractions('outside-date', outsideDate, outsideDate, [
      'default',
      'hover',
    ])
  })

  test('selected normal date', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDefaultDate')
    await page.getByRole('textbox').click()

    const selectedDate = page.getByRole('button', {
      name: '5 January 2023',
      exact: true,
    })
    await selectedDate.click()
    await openCalendar(page)

    await storybook.testInteractions(
      'selected-normal-date',
      selectedDate,
      selectedDate,
      ['default', 'hover']
    )
  })

  test('selected weekend date', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDefaultDate')
    await page.getByRole('textbox').click()

    const selectedDate = page.getByRole('button', {
      name: '7 January 2023',
      exact: true,
    })
    await selectedDate.click()
    await openCalendar(page)

    await storybook.testInteractions(
      'selected-weekend-date',
      selectedDate,
      selectedDate,
      ['default', 'hover']
    )
  })

  test('selected outside date', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDefaultDate')
    await page.getByRole('textbox').click()

    const selectedDate = page.getByRole('button', {
      name: '29 December 2022',
      exact: true,
    })
    await selectedDate.click()
    await openCalendar(page)

    await storybook.testInteractions(
      'selected-outside-date',
      selectedDate,
      selectedDate,
      ['default', 'hover']
    )
  })

  test('disabled normal date', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'HiddenDisabledDate')
    await openCalendar(page)

    await expect(page.getByRole('table')).toHaveScreenshot()
  })
})
