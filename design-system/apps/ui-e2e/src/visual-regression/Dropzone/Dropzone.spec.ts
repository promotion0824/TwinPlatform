import { test, expect } from '@willowinc/playwright'

const componentName = 'Dropzone'
const groupName = 'Inputs'

// eslint-disable-next-line playwright/expect-expect
test('idle', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await storybook.testInteractions(
    'idle',
    page.locator('.mantine-Dropzone-root'),
    storybook.storyContainer,
    ['default', 'focus', 'hover']
  )
})

test('loading', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Loading')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('disabled', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Disabled')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

// eslint-disable-next-line playwright/expect-expect
test('invalid', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'invalid')
  await storybook.testInteractions(
    'invalid',
    page.locator('.mantine-Dropzone-root'),
    storybook.storyContainer,
    ['default', 'focus', 'hover']
  )
})

test('description', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'FileRestrictions')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('accept', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'Accept')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('reject', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'Reject')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
