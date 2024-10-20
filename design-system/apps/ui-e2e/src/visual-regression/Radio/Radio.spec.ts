import { test, expect } from '@willowinc/playwright'

const componentName = 'Radio'
const groupName = 'Inputs'

test('default', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  await storybook.testInteractions(
    'unchecked',
    page.locator('label'),
    storybook.storyContainer,
    ['default', 'focus']
  )
})

test('default checked', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Checked')

  await storybook.testInteractions(
    'checked',
    page.locator('label'),
    storybook.storyContainer,
    ['default', 'focus']
  )
})

test('disabled checked', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')

  const story = storybook.storyContainer
  await expect(story).toHaveScreenshot()
})

test('invalid checked', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Invalid')

  const story = storybook.storyContainer
  await expect(story).toHaveScreenshot()
})
