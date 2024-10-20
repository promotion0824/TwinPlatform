import { test, expect } from '@willowinc/playwright'

const componentName = 'SwitchGroup'
const groupName = 'Inputs'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('no label', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'NoLabel')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('description', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Description')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('invalid', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Invalid')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('error', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Error')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('required', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Required')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('inline', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Inline')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('inline description', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'InlineDescription')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('inline error', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'InlineError')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
