import { test, expect } from '@willowinc/playwright'

const componentName = 'Switch'
const groupName = 'inputs'

test('no label', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'NoLabel')
  const switchElement = page.locator('input')
  await storybook.testInteractions(
    'Switch',
    switchElement,
    storybook.storyContainer,
    ['default', 'focus']
  )
})

test('label on right', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LabelRight')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('label on left', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LabelLeft')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('with justify', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Justify')
  const stories = storybook.storyRoot.getByTestId('switch-input')

  const justifyValues = ['flex-start', 'flex-end', 'space-between']

  for (let i = 0; i < justifyValues.length; i++)
    await expect(stories.nth(i)).toHaveScreenshot(
      `with-justify-${justifyValues[i]}.png`
    )
})

test('long label', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LabelLong')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled (checked)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'DisabledChecked')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled (unchecked)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'DisabledUnchecked')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('error (checked)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'ErrorChecked')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('error (unchecked)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'ErrorUnchecked')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
