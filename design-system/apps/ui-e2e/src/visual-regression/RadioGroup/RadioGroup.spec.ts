import { test, expect } from '@willowinc/playwright'

const componentName = 'RadioGroup'
const groupName = 'Inputs'

test('default', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  const story = page.locator('#storybook-root')
  await expect(story).toHaveScreenshot('default.png')

  const radioOne = page.getByLabel('Twins')
  await radioOne.click()
  await expect(storybook.storyContainer).toHaveScreenshot('selected.png')
})

test('disabled', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('invalid', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'InvalidWithError')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('inline', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'inline')

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
