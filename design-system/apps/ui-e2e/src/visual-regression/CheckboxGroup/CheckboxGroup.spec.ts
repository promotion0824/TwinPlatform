import { test, expect } from '@willowinc/playwright'

const componentName = 'CheckboxGroup'
const groupName = 'Inputs'

test('default checkbox group', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('checkbox group with error message', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'RequiredGroupOfInvalidCheckboxWithErrorMessage'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('inline checkbox group with error message', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(
    componentName,
    'RequiredInlineCheckboxGroupWithErrorMessage'
  )

  await expect(storybook.storyRoot).toHaveScreenshot(
    'inline-checkbox-group-full-width.png'
  )

  await page.setViewportSize({ width: 150, height: 200 })
  await expect(storybook.storyRoot).toHaveScreenshot(
    'inline-checkbox-group-auto-wrap.png'
  )
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})
