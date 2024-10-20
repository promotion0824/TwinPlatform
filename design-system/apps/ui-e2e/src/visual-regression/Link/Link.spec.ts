import { test, expect } from '@willowinc/playwright'

const componentName = 'Link'
const groupName = 'Navigation'

// eslint-disable-next-line playwright/expect-expect
test('default', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  const link = page.getByText('Willow Design System')
  await storybook.testInteractions('link', link, link, [
    'default',
    'hover',
    'focus',
    'active',
  ])
})

test('sizes', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Sizes')

  const sizes = ['xs', 'sm', 'md', 'lg']
  for (const size of sizes) {
    const testId = `link-size-${size}`
    const link = page.getByTestId(testId)
    await expect(link).toHaveScreenshot(`${testId}.png`)
  }
})

// eslint-disable-next-line playwright/expect-expect
test('prefix', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Prefix')
  const link = page.getByTestId('link-prefix')
  await storybook.testInteractions('link-prefix', link, link, [
    'default',
    'hover',
    'focus',
    'active',
  ])
})

// eslint-disable-next-line playwright/expect-expect
test('suffix', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Suffix')
  const link = page.getByTestId('link-suffix')
  await storybook.testInteractions('link-suffix', link, link, [
    'default',
    'hover',
    'focus',
    'active',
  ])
})
