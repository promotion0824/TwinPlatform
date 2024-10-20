import { test, expect } from '@willowinc/playwright'

const componentName = 'Sidebar'
const groupName = 'UI Chrome'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('hovered', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  await page.getByText('Home').first().hover()
  await expect(storybook.storyContainer).toHaveScreenshot('hovered-active.png')

  await page.getByText('Dashboards').first().hover()
  await expect(storybook.storyContainer).toHaveScreenshot('hovered-default.png')
})

test('collapsed', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'CollapsedByDefault')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('multiple groups', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'MultipleGroups')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('fill', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Fill')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('overflow', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'Overflow')
  await expect(storybook.storyContainer).toHaveScreenshot('overflow.png')

  await page.getByText('first_page').click()
  await expect(storybook.storyContainer).toHaveScreenshot(
    'overflow-collapsed.png'
  )
})

test('no footer', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'HideFooter')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
