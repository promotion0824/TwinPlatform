import { test, expect } from '@willowinc/playwright'

const componentName = 'TextInput'
const groupName = 'Inputs'

test('placeholder', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'playground')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('interaction styles with default value', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithDefaultValue')

  await storybook.testInteractions(
    'default value',
    page.getByLabel('Label'),
    undefined,
    ['default', 'hover', 'focus']
  )
})

test('required', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Required')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('readonly can not be edited', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Readonly')

  await page.getByLabel('Label').type('new value')

  await expect(storybook.storyRoot).toHaveScreenshot('readonly.png')
})

test('invalid', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Invalid')

  const input = storybook.storyRoot
  await expect(input).toHaveScreenshot('invalid with placeholder.png')

  await page.getByLabel('Label').type('new value')
  await expect(input).toHaveScreenshot('invalid with value.png')
})

test('disabled cannot be edited', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Disabled')

  await page.getByLabel('Label').type('new value')

  await expect(storybook.storyRoot).toHaveScreenshot('disabled.png')
})

test('prefix and suffix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'WithPrefixAndSuffix')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('clearable', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Clearable')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('responsiveness with label and error message', async ({ storybook }) => {
  await storybook.gotoHidden(
    'TextInput',
    'ResponsiveWithLongLabelAndErrorMessage'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})
