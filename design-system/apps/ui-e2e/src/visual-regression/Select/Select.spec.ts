import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'Select'
const groupName = 'Inputs'

const input = (page: Page) => page.getByRole('textbox')
test('selected value with another option hover', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'playground')

  await page.getByRole('option', { name: 'Svelte' }).hover()

  // The difference in hover color is too subtle,
  // hence we need to set the threshold to 0 here
  await expect(storybook.storyRoot).toHaveScreenshot({
    threshold: 0,
  })
})

test('with disabled option hover', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'DisabledOption')

  await input(page).click() // open dropdown

  // getByRole with {disabled: boolean} not working here,
  // all options are considered not disabled, not sure why.
  await page.getByRole('option', { name: 'React' }).hover()
  // The difference in hover color is too subtle,
  // hence we need to set the threshold to 0 here to monitor the difference
  await expect(storybook.storyRoot).toHaveScreenshot({
    threshold: 0,
  })
})

test('disabled select', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Disabled')

  const select = input(page)

  // none of the following interactions should change the select style
  await storybook.testInteractions('disabled', select, select, [
    'default',
    'hover',
    'focus',
  ])
})

test('readonly select', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Readonly')

  const select = input(page)

  // none of the following interactions should change the select style
  await storybook.testInteractions('readonly', select, select, [
    'default',
    'click',
    'hover',
    'focus',
  ])
})

test('with label and error message', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithLabelAndErrorMessage')

  await expect(
    storybook.storyRoot.locator('div').first().locator('div').first()
  ).toHaveScreenshot()

  const select = input(page)
  // invalid select can not be hovered or focused
  await storybook.testInteractions('invalid', select, select, [
    'hover',
    'focus',
  ])
  // invalid select can only be clicked
  await storybook.testInteractions('invalid', select, storybook.storyRoot, [
    'click',
  ])
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('grouped items', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'GroupingItems')

  await input(page).click() // open dropdown

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('prefix and suffix', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PrefixAndSuffix')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('long value should not overlap icons', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LongValue')

  await expect(storybook.storyRoot.locator('div').first()).toHaveScreenshot()
})

test('scrollable area', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'WithScrollbar')

  await page.getByRole('option', { name: 'long' }).hover()
  await expect(storybook.storyRoot.locator('div').first()).toHaveScreenshot()
})
