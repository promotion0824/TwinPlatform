import { test, expect } from '@willowinc/playwright'

const componentName = 'NumberInput'
const groupName = 'Inputs'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('label', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Label')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('description', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Description')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('placeholder', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Placeholder')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('placeholder invalid', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PlaceholderInvalid')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('placeholder disabled', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PlaceholderDisabled')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('placeholder read only', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PlaceholderReadOnly')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('required', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Required')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('invalid', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Invalid')
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

test('disabled', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('read only', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ReadOnly')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('prefix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Prefix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('prefix invalid', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PrefixInvalid')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('prefix disabled', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PrefixDisabled')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('prefix read only', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PrefixReadOnly')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('suffix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Suffix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('text prefix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TextPrefix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('text suffix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TextSuffix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})
