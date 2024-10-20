import { test, expect } from '@willowinc/playwright'

const componentName = 'NavList'
const groupName = 'Navigation'

test('default/focus/hover', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  const roleLink = page.locator('a').filter({ hasText: 'Roles' })
  await storybook.testInteractions(
    'NavList',
    roleLink,
    storybook.storyContainer,
    ['default', 'focus', 'hover']
  )
})

test('icons', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Icons')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('nested NavLists', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Nested NavLists')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('collapsed parent with active child', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Nested NavLists')

  await page.getByText('Authorization').click()

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('multiple nested NavLists', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'MultipleNestedNavLists')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('groups', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Groups')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
