import { test, expect } from '@willowinc/playwright'

const componentName = 'PageTitle'
const groupName = 'Navigation'

test('PageTitleItem with interactions', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  const pageTitleItem = page.getByRole('link')
  await storybook.testInteractions(
    'PageTitleItem',
    pageTitleItem,
    pageTitleItem,
    ['default', 'focus', 'hover']
  )
})

test('active PageTitleItem with interactions', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'TwoItems')
  const pageTitleItem = page.getByRole('link', { name: 'Page 2' })
  await storybook.testInteractions(
    'active-PageTitleItem',
    pageTitleItem,
    pageTitleItem,
    ['default', 'focus', 'hover']
  )
})

test('two items', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'TwoItems')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('three items', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ThreeItems')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('overflow menu', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'OverflowMenu')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('overflow menu (opened)', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'OverflowMenu')
  await page.getByText('more_horiz').click()
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('max items', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'MaxItems')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('suffix and prefix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'SuffixAndPrefix')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
