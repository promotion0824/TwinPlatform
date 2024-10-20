import { test, expect } from '@willowinc/playwright'

const componentName = 'Checkbox'
const groupName = 'Inputs'

test('default with focus', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithLabel')

  const checkbox = page.getByLabel('Label')
  await checkbox.focus()
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('checked', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'checked')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('indeterminate', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Indeterminate')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('indeterminate with checked', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'IndeterminateChecked')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'disabled')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled with checked', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'DisabledChecked')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled with indeterminate', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'DisabledIndeterminate')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('invalid', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'Invalid')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('with justify', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Justify')
  const stories = storybook.storyRoot.getByTestId('willow-checkbox')

  const justifyValues = ['flex-start', 'flex-end', 'space-between']

  for (let i = 0; i < justifyValues.length; i++)
    await expect(stories.nth(i)).toHaveScreenshot(
      `with-justify-${justifyValues[i]}.png`
    )
})
