import { test, expect } from '@willowinc/playwright'

const componentName = 'AvatarGroup'
const groupName = 'Data Display'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'playground')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('has overflow', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'HasOverflow')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('with tooltip', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithTooltip')
  const avatar = storybook.storyContainer.getByText('AX')
  await avatar.hover()

  await expect(page.getByText('Tooltip 3')).toBeVisible()
})
